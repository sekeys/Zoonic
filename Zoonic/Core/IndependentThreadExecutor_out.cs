using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zoonic.Interface;
using Zoonic.Lib;
using Zoonic.Lib.Collection;

namespace Zoonic.Concurrency
{
    public class IndependentThreadExecutor_out : Executor
    {

        static readonly TimeSpan DefaultBreakoutInterval = TimeSpan.FromMilliseconds(100);
        const int ST_NOT_STARTED = 1;
        const int ST_STARTED = 2;
        const int ST_SHUTTING_DOWN = 3;
        const int ST_SHUTDOWN = 4;
        const int ST_TERMINATED = 5;
        const string DefaultWorkerThreadName = "IndependentThreadExecutor worker";

        private int executionState;
        private Thread thread;
        readonly PreciseTimeSpan preciseBreakoutInterval;
        PreciseTimeSpan lastExecutionTime;
        readonly ManualResetEventSlim emptyEvent = new ManualResetEventSlim(false, 1);
        readonly TaskScheduler scheduler;
        readonly TaskCompletionSource terminationCompletionSource;
        PreciseTimeSpan gracefulShutdownStartTime;
        PreciseTimeSpan gracefulShutdownQuietPeriod;
        PreciseTimeSpan gracefulShutdownTimeout;
        readonly IConcurrentQueue<IRunnable> taskQueue;

        public override bool IsShuttingDown => this.executionState >= ST_SHUTTING_DOWN;

        public override Task TerminationCompletion => this.terminationCompletionSource.Task;

        public override bool IsShutdown => this.executionState >= ST_SHUTDOWN;

        public override bool IsTerminated => this.executionState == ST_TERMINATED;

        public override bool IsInLoop(Thread t) => this.thread == t;

        #region constructor
        public IndependentThreadExecutor_out(string threadName, TimeSpan breakoutInterval)
            : this(null, threadName, breakoutInterval, new FriendlyConcurrentQueue<IRunnable>())
        {
        }
        public IndependentThreadExecutor_out(IExecutorGroup parent)
            : this(parent, null, DefaultBreakoutInterval)
        {
        }
        /// <summary>Creates a new instance of <see cref="SingleThreadEventExecutor"/>.</summary>
        public IndependentThreadExecutor_out(IExecutorGroup parent, string threadName, TimeSpan breakoutInterval)
            : this(parent, threadName, breakoutInterval, new FriendlyConcurrentQueue<IRunnable>())
        {
        }

        protected IndependentThreadExecutor_out(string threadName, TimeSpan breakoutInterval, IConcurrentQueue<IRunnable> taskQueue)
            : this(null, threadName, breakoutInterval, taskQueue)
        { }

        protected IndependentThreadExecutor_out(IExecutorGroup parent, string threadName, TimeSpan breakoutInterval, IConcurrentQueue<IRunnable> taskQueue)
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

        private void Loop(object obj)
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
                        this.executionState = ST_TERMINATED;
                        this.terminationCompletionSource.TrySetException(ex);
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                this.scheduler);
        }
        #endregion
        /// <inheritdoc cref="IEventExecutor"/>
        public override void Execute(IRunnable task)
        {
            this.taskQueue.TryEnqueue(task);

            if (!this.InLoop)
            {
                this.emptyEvent.Set();
            }
        }
        
        protected void WakeUp(bool InLoop)
        {
            if (!InLoop || (this.executionState == ST_SHUTTING_DOWN))
            {
                this.Execute(new NoOpRunnable());
            }
        }

        /// <inheritdoc cref="IEventExecutor"/>
        public override Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout)
        {

            if (this.IsShuttingDown)
            {
                return this.TerminationCompletion;
            }

            bool InLoop = this.InLoop;
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
                if (InLoop)
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
                this.WakeUp(InLoop);
            }

            return this.TerminationCompletion;
        }

        protected bool ConfirmShutdown()
        {
            if (!this.IsShuttingDown)
            {
                return false;
            }

            if (this.InLoop) { throw new Exception("must be invoked from an loop"); }

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
            if (success && (this.gracefulShutdownStartTime == PreciseTimeSpan.Zero))
            {
                //Logger.Error(
                //    $"Buggy {typeof(IEventExecutor).Name} implementation; {typeof(SingleThreadEventExecutor).Name}.ConfirmShutdown() must be called "
                //    + "before run() implementation terminates.");
            }

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
                    if (!this.taskQueue.IsEmpty)
                    {
                        //Logger.Warn($"An event executor terminated with non-empty task queue ({this.taskQueue.Count})");
                    }

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
            
            IRunnable task;
            if (!this.taskQueue.TryDequeue(out task))
            {
                this.emptyEvent.Reset();
                if (!this.taskQueue.TryDequeue(out task) && !this.IsShuttingDown) 
                {
                    IScheduledRunnable nextScheduledTask = this.ScheduledTaskQueue.Peek();
                    if (nextScheduledTask != null)
                    {
                        PreciseTimeSpan wakeupTimeout = nextScheduledTask.Deadline - PreciseTimeSpan.FromStart;
                        if (wakeupTimeout.Ticks > 0)
                        {
                            if (this.emptyEvent.Wait(wakeupTimeout.ToTimeSpan()))
                            {
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
