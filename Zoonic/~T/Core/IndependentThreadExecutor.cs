﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Concurrency
{
    using Zoonic.Interface;
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Threading.Tasks;
    using Zoonic.Lib;
    using Zoonic.Lib.Collection;

    /// <summary>
    /// <see cref="IEventExecutor"/> backed by a single thread.
    /// </summary>
    public class IndependentThreadExecutor<T> : Executor<T>
    {

        const int ST_NOT_STARTED = 1;
        const int ST_STARTED = 2;
        const int ST_SHUTTING_DOWN = 3;
        const int ST_SHUTDOWN = 4;
        const int ST_TERMINATED = 5;
        const string DefaultWorkerThreadName = "IndependentThreadExecutor~T worker";

        static readonly IRunnable WAKEUP_TASK = new NoOpRunnable();

        //static readonly IInternalLogger Logger =
        //    InternalLoggerFactory.GetInstance<SingleThreadEventExecutor>();

        readonly IConcurrentQueue<IRunnable<T>> taskQueue;
        readonly Thread thread;
        volatile int executionState = ST_NOT_STARTED;
        readonly PreciseTimeSpan preciseBreakoutInterval;
        PreciseTimeSpan lastExecutionTime;
        readonly ManualResetEventSlim emptyEvent = new ManualResetEventSlim(false, 1);
        readonly TaskScheduler scheduler;
        readonly TaskCompletionSource terminationCompletionSource;
        PreciseTimeSpan gracefulShutdownStartTime;
        PreciseTimeSpan gracefulShutdownQuietPeriod;
        PreciseTimeSpan gracefulShutdownTimeout;

        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutor"/>.</summary>
        public IndependentThreadExecutor(string threadName, TimeSpan breakoutInterval)
            : this(null, threadName, breakoutInterval, new FriendlyConcurrentQueue<IRunnable<T>>())
        {
        }

        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutor"/>.</summary>
        public IndependentThreadExecutor(IExecutorGroup parent, string threadName, TimeSpan breakoutInterval)
            : this(parent, threadName, breakoutInterval, new FriendlyConcurrentQueue<IRunnable>())
        {
        }

        protected IndependentThreadExecutor(string threadName, TimeSpan breakoutInterval, IConcurrentQueue<IRunnable> taskQueue)
            : this(null, threadName, breakoutInterval, taskQueue)
        { }

        protected IndependentThreadExecutor(IExecutorGroup parent, string threadName, TimeSpan breakoutInterval, IConcurrentQueue<IRunnable> taskQueue)
            : base(parent)
        {
            this.terminationCompletionSource = new TaskCompletionSource();
            this.taskQueue = taskQueue;
            this.preciseBreakoutInterval = PreciseTimeSpan.FromTimeSpan(breakoutInterval);
            this.scheduler = new ExecutorTaskScheduler(this);
            this.thread = new Thread(this.Loop);
            if (string.IsNullOrEmpty(threadName))
            {
                this.thread.Name = DefaultWorkerThreadName;
            }
            else
            {
                this.thread.Name = threadName;
            }
            this.thread.Start();
        }

        /// <summary>
        ///     Task Scheduler that will post work to this executor's queue.
        /// </summary>
        public TaskScheduler Scheduler => this.scheduler;

        void Loop()
        {
            this.SetCurrentExecutor(this);

            Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        Interlocked.CompareExchange(ref this.executionState, ST_STARTED, ST_NOT_STARTED);
                        while (!this.ConfirmShutdown())
                        {
                            this.RunAllTasks(this.preciseBreakoutInterval);
                        }
                        this.CleanupAndTerminate(true);
                    }
                    catch (Exception ex)
                    {
                        //Logger.Error("{}: execution loop failed", this.thread.Name, ex);
                        this.executionState = ST_TERMINATED;
                        this.terminationCompletionSource.TrySetException(ex);
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                this.scheduler);
        }

        /// <inheritdoc cref="IEventExecutor"/>
        public override bool IsShuttingDown => this.executionState >= ST_SHUTTING_DOWN;

        /// <inheritdoc cref="IEventExecutor"/>
        public override Task TerminationCompletion => this.terminationCompletionSource.Task;

        /// <inheritdoc cref="IEventExecutor"/>
        public override bool IsShutdown => this.executionState >= ST_SHUTDOWN;

        /// <inheritdoc cref="IEventExecutor"/>
        public override bool IsTerminated => this.executionState == ST_TERMINATED;

        /// <inheritdoc cref="IEventExecutor"/>
        public override bool IsInLoop(Thread t) => this.thread == t;

        /// <inheritdoc cref="IEventExecutor"/>
        public override void Execute(IRunnable task)
        {
            this.taskQueue.TryEnqueue(task);

            if (!this.InLoop)
            {
                this.emptyEvent.Set();
            }
        }

        protected void WakeUp(bool inEventLoop)
        {
            if (!inEventLoop || (this.executionState == ST_SHUTTING_DOWN))
            {
                this.Execute(WAKEUP_TASK);
            }
        }

        /// <inheritdoc cref="IEventExecutor"/>
        public override Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {
            Contract.Requires(quietPeriod >= TimeSpan.Zero);
            Contract.Requires(timeout >= quietPeriod);

            if (this.IsShuttingDown)
            {
                return this.TerminationCompletion;
            }

            bool inEventLoop = this.InLoop;
            bool wakeup;
            int oldState;
            while (true)
            {
                if (this.IsShuttingDown)
                {
                    return this.TerminationCompletion;
                }
                int newState;
                wakeup = true;
                oldState = this.executionState;
                if (inEventLoop)
                {
                    newState = ST_SHUTTING_DOWN;
                }
                else
                {
                    switch (oldState)
                    {
                        case ST_NOT_STARTED:
                        case ST_STARTED:
                            newState = ST_SHUTTING_DOWN;
                            break;
                        default:
                            newState = oldState;
                            wakeup = false;
                            break;
                    }
                }
                if (Interlocked.CompareExchange(ref this.executionState, newState, oldState) == oldState)
                {
                    break;
                }
            }
            this.gracefulShutdownQuietPeriod = PreciseTimeSpan.FromTimeSpan(quietPeriod);
            this.gracefulShutdownTimeout = PreciseTimeSpan.FromTimeSpan(timeout);

            // todo: revisit
            //if (oldState == ST_NOT_STARTED)
            //{
            //    scheduleExecution();
            //}

            if (wakeup)
            {
                this.WakeUp(inEventLoop);
            }

            return this.TerminationCompletion;
        }

        protected bool ConfirmShutdown()
        {
            if (!this.IsShuttingDown)
            {
                return false;
            }

            Contract.Assert(this.InLoop, "must be invoked from an event loop");

            this.CancelScheduledTasks();

            if (this.gracefulShutdownStartTime == PreciseTimeSpan.Zero)
            {
                this.gracefulShutdownStartTime = PreciseTimeSpan.FromStart;
            }

            if (this.RunAllTasks()) // || runShutdownHooks())
            {
                if (this.IsShutdown)
                {
                    // Executor shut down - no new tasks anymore.
                    return true;
                }

                // There were tasks in the queue. Wait a little bit more until no tasks are queued for the quiet period.
                this.WakeUp(true);
                return false;
            }

            PreciseTimeSpan nanoTime = PreciseTimeSpan.FromStart;

            if (this.IsShutdown || (nanoTime - this.gracefulShutdownStartTime > this.gracefulShutdownTimeout))
            {
                return true;
            }

            if (nanoTime - this.lastExecutionTime <= this.gracefulShutdownQuietPeriod)
            {
                // Check if any tasks were added to the queue every 100ms.
                // TODO: Change the behavior of takeTask() so that it returns on timeout.
                // todo: ???
                this.WakeUp(true);
                Thread.Sleep(100);

                return false;
            }

            // No tasks were added for last quiet period - hopefully safe to shut down.
            // (Hopefully because we really cannot make a guarantee that there will be no execute() calls by a user.)
            return true;
        }

        protected void CleanupAndTerminate(bool success)
        {
            while (true)
            {
                int oldState = this.executionState;
                if ((oldState >= ST_SHUTTING_DOWN) || (Interlocked.CompareExchange(ref this.executionState, ST_SHUTTING_DOWN, oldState) == oldState))
                {
                    break;
                }
            }

            // Check if confirmShutdown() was called at the end of the loop.
            //if (success && (this.gracefulShutdownStartTime == PreciseTimeSpan.Zero))
            //{
            //    Logger.Error(
            //        $"Buggy {typeof(IEventExecutor).Name} implementation; {typeof(SingleThreadEventExecutor).Name}.ConfirmShutdown() must be called "
            //        + "before run() implementation terminates.");
            //}

            try
            {
                // Run all remaining tasks and shutdown hooks.
                while (true)
                {
                    if (this.ConfirmShutdown())
                    {
                        break;
                    }
                }
            }
            finally
            {
                try
                {
                    this.Cleanup();
                }
                finally
                {
                    Interlocked.Exchange(ref this.executionState, ST_TERMINATED);
                    //if (!this.taskQueue.IsEmpty)
                    //{
                    //    //Logger.Warn($"An event executor terminated with non-empty task queue ({this.taskQueue.Count})");
                    //}

                    //firstRun = true;
                    this.terminationCompletionSource.Complete();
                }
            }
        }

        protected virtual void Cleanup()
        {
            // NOOP
        }

        protected bool RunAllTasks()
        {
            this.FetchFromScheduledTaskQueue();
            IRunnable task = this.PollTask();
            if (task == null)
            {
                return false;
            }

            while (true)
            {
                try
                {
                    task.Run();
                }
                catch (Exception ex)
                {
                    //Logger.Warn("A task raised an exception.", ex);
                }

                task = this.PollTask();
                if (task == null)
                {
                    this.lastExecutionTime = PreciseTimeSpan.FromStart;
                    return true;
                }
            }
        }

        bool RunAllTasks(PreciseTimeSpan timeout)
        {
            this.FetchFromScheduledTaskQueue();
            IRunnable task = this.PollTask();
            if (task == null)
            {
                return false;
            }

            PreciseTimeSpan deadline = PreciseTimeSpan.Deadline(timeout);
            long runTasks = 0;
            PreciseTimeSpan executionTime;
            while (true)
            {
                try
                {
                    task.Run();
                }
                catch (Exception ex)
                {
                    //Logger.Warn("A task raised an exception.", ex);
                }

                runTasks++;

                // Check timeout every 64 tasks because nanoTime() is relatively expensive.
                // XXX: Hard-coded value - will make it configurable if it is really a problem.
                if ((runTasks & 0x3F) == 0)
                {
                    executionTime = PreciseTimeSpan.FromStart;
                    if (executionTime >= deadline)
                    {
                        break;
                    }
                }

                task = this.PollTask();
                if (task == null)
                {
                    executionTime = PreciseTimeSpan.FromStart;
                    break;
                }
            }

            this.lastExecutionTime = executionTime;
            return true;
        }

        bool FetchFromScheduledTaskQueue()
        {
            PreciseTimeSpan nanoTime = PreciseTimeSpan.FromStart;
            IScheduledRunnable scheduledTask = this.PollScheduledTask(nanoTime);
            while (scheduledTask != null)
            {
                if (!this.taskQueue.TryEnqueue(scheduledTask))
                {
                    // No space left in the task queue add it back to the scheduledTaskQueue so we pick it up again.
                    this.ScheduledTaskQueue.Enqueue(scheduledTask);
                    return false;
                }
                scheduledTask = this.PollScheduledTask(nanoTime);
            }
            return true;
        }

        IRunnable PollTask()
        {
            Contract.Assert(this.InLoop);

            IRunnable task;
            if (!this.taskQueue.TryDequeue(out task))
            {
                this.emptyEvent.Reset();
                if (!this.taskQueue.TryDequeue(out task) && !this.IsShuttingDown) // revisit queue as producer might have put a task in meanwhile
                {
                    IScheduledRunnable nextScheduledTask = this.ScheduledTaskQueue.Peek();
                    if (nextScheduledTask != null)
                    {
                        PreciseTimeSpan wakeupTimeout = nextScheduledTask.Deadline - PreciseTimeSpan.FromStart;
                        if (wakeupTimeout.Ticks > 0)
                        {
                            if (this.emptyEvent.Wait(wakeupTimeout.ToTimeSpan()))
                            {
                                // woken up before the next scheduled task was due
                                this.taskQueue.TryDequeue(out task);
                            }
                        }
                    }
                    else
                    {
                        this.emptyEvent.Wait();
                        this.taskQueue.TryDequeue(out task);
                    }
                }
            }

            return task;
        }

        sealed class NoOpRunnable : IRunnable
        {
            public void Run()
            {
            }
        }
    }
}
