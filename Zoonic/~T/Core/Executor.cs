


namespace Zoonic.Concurrency
{
    using Zoonic.Interface;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Zoonic.Lib.Collection;
    using Zoonic.Lib;

    public abstract class Executor<T> : IExecutor<T>
    {
        protected Executor():this(null)
        {
        }

        public IExecutorGroup<T> Parent { get; private set; }
        protected Executor(IExecutorGroup<T> parent)
        {
            this.Parent = parent;
        }
        

        public bool InLoop => IsInLoop(Thread.CurrentThread);

        protected PriorityQueue<IScheduledRunnable> ScheduledTaskQueue = new PriorityQueue<IScheduledRunnable>();

        internal void RemoveScheduled(IScheduledRunnable task)
        {
            if (this.InLoop)
            {
                this.ScheduledTaskQueue.Remove(task);
            }
            else
            {
                this.Execute((e, t) => ((Executor)e).ScheduledTaskQueue.Remove((IScheduledRunnable)t), this, task);
            }
        }


        protected static bool IsNullOrEmpty<T>(PriorityQueue<T> taskQueue)
           where T : class
        {
            return taskQueue == null || taskQueue.Count == 0;
        }
        protected virtual void CancelScheduledTasks()
        {
            PriorityQueue<IScheduledRunnable> scheduledTaskQueue = this.ScheduledTaskQueue;
            if (IsNullOrEmpty(scheduledTaskQueue))
            {
                return;
            }

            IScheduledRunnable[] tasks = scheduledTaskQueue.ToArray();
            foreach (IScheduledRunnable t in tasks)
            {
                t.Cancel();
            }

            this.ScheduledTaskQueue.Clear();
        }

        #region Property
        public abstract Task TerminationCompletion { get; }
        public abstract bool IsShuttingDown { get; }
        public abstract bool IsShutdown { get; }
        public abstract bool IsTerminated { get; }

        public abstract bool IsInLoop(Thread thread);
        #endregion

        #region Execute Method
        public abstract void Execute(IRunnable task);

        public void Execute(Action<object> action, object state) => this.Execute(new StateActionTaskQueueNode(action, state));

        public void Execute(Action<object, object> action, object context, object state) => this.Execute(new StateActionWithContextTaskQueueNode(action, context, state));

        public void Execute(Action action) => this.Execute(new ActionTaskQueueNode(action));
        #endregion


        protected IScheduledRunnable PollScheduledTask() => this.PollScheduledTask(PreciseTimeSpan.FromStart);

        protected IScheduledRunnable PollScheduledTask(PreciseTimeSpan nanoTime)
        {
            IScheduledRunnable scheduledTask = this.ScheduledTaskQueue.Peek();
            if (scheduledTask == null)
            {
                return null;
            }
            if (scheduledTask.Deadline <= nanoTime)
            {
                this.ScheduledTaskQueue.Dequeue();
                return scheduledTask;
            }
            return null;
        }

        protected PreciseTimeSpan NextScheduledTaskNanos()
        {
            IScheduledRunnable nextScheduledRunnable = this.PeekScheduledTask();
            return nextScheduledRunnable == null ? PreciseTimeSpan.MinusOne : nextScheduledRunnable.Deadline;
        }

        protected IScheduledRunnable PeekScheduledTask()
        {
            PriorityQueue<IScheduledRunnable> scheduledTaskQueue = this.ScheduledTaskQueue;
            return IsNullOrEmpty(scheduledTaskQueue) ? null : scheduledTaskQueue.Peek();
        }

        protected bool HasScheduledTasks()
        {
            IScheduledRunnable scheduledTask = this.ScheduledTaskQueue.Peek();
            return scheduledTask != null && scheduledTask.Deadline <= PreciseTimeSpan.FromStart;
        }

        #region Shedule Method
        public virtual Task ScheduleAsync(Action action, TimeSpan delay) => this.ScheduleAsync(action, delay, CancellationToken.None);
        public virtual Task ScheduleAsync(Action<object> action, object state, TimeSpan delay) => this.ScheduleAsync(action, state, delay, CancellationToken.None);
        public virtual Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay) => this.ScheduleAsync(action, context, state, delay, CancellationToken.None);
        public virtual IScheduledTask Schedule(IRunnable action, TimeSpan delay)
        {
            Require.NotNull(action);

            return this.Schedule(new RunnableScheduledTask<T>(this, action, PreciseTimeSpan.Deadline(delay)));
        }

        public virtual IScheduledTask Schedule(Action action, TimeSpan delay)
        {
            Require.NotNull(action);

            return this.Schedule(new ActionScheduledTask(this, action, PreciseTimeSpan.Deadline(delay)));
        }

        public virtual IScheduledTask Schedule(Action<object> action, object state, TimeSpan delay)
        {
            Require.NotNull(action);
            return this.Schedule(new StateActionScheduledTask(this, action, state, PreciseTimeSpan.Deadline(delay)));
        }

        public virtual IScheduledTask Schedule(Action<object, object> action, object context, object state, TimeSpan delay)
        {
            Require.NotNull(action);

            return this.Schedule(new StateActionWithContextScheduledTask(this, action, context, state, PreciseTimeSpan.Deadline(delay)));
        }

        public virtual Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken cancellationToken)
        {
            Require.NotNull(action);

            if (cancellationToken.IsCancellationRequested)
            {
                return TaskEx.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return this.Schedule(action, delay).Completion;
            }

            return this.Schedule(new FuncScheduledAsyncTask(this, action, PreciseTimeSpan.Deadline(delay), cancellationToken)).Completion;
        }

        public virtual Task ScheduleAsync(Action<object> action, object state, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskEx.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return this.Schedule(action, state, delay).Completion;
            }

            return this.Schedule(new StateActionScheduledAsyncTask(this, action, state, PreciseTimeSpan.Deadline(delay), cancellationToken)).Completion;
        }

        public virtual Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TaskEx.Cancelled;
            }

            if (!cancellationToken.CanBeCanceled)
            {
                return this.Schedule(action, context, state, delay).Completion;
            }

            return this.Schedule(new StateWithContextScheduledAsyncTask(this, action, context, state, PreciseTimeSpan.Deadline(delay), cancellationToken)).Completion;
        }

        protected IScheduledRunnable Schedule(IScheduledRunnable task)
        {
            if (this.InLoop)
            {
                this.ScheduledTaskQueue.Enqueue(task);
            }
            else
            {
                this.Execute((e, t) => ((Executor)e).ScheduledTaskQueue.Enqueue((IScheduledRunnable)t), this, task);
            }
            return task;
        }
        #endregion

        #region Submit Method

        public Task<T> SubmitAsync<T>(Func<T> func) => this.SubmitAsync(func, CancellationToken.None);

        public Task<T> SubmitAsync<T>(Func<T> func, CancellationToken cancellationToken)
        {
            var node = new FuncSubmitQueueNode<T>(func, cancellationToken);
            this.Execute(node);
            return node.Completion;
        }

        public Task<T> SubmitAsync<T>(Func<object, T> func, object state) => this.SubmitAsync(func, state, CancellationToken.None);

        public Task<T> SubmitAsync<T>(Func<object, T> func, object state, CancellationToken cancellationToken)
        {
            var node = new StateFuncSubmitQueueNode<T>(func, state, cancellationToken);
            this.Execute(node);
            return node.Completion;
        }

        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state) => this.SubmitAsync(func, context, state, CancellationToken.None);

        public Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state, CancellationToken cancellationToken)
        {
            var node = new StateFuncWithContextSubmitQueueNode<T>(func, context, state, cancellationToken);
            this.Execute(node);
            return node.Completion;
        }

        #endregion



        static readonly TimeSpan DefaultShutdownQuietPeriod = TimeSpan.FromSeconds(2);

        static readonly TimeSpan DefaultShutdownTimeout = TimeSpan.FromSeconds(15);

        public Task ShutdownGracefullyAsync() => this.ShutdownGracefullyAsync(DefaultShutdownQuietPeriod, DefaultShutdownTimeout);
        
        public abstract Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout);
        
        protected void SetCurrentExecutor(IExecutor executor) => ExecutionEnvironment.SetCurrentExecutor(executor);

        #region Queuing data structures

        sealed class ActionTaskQueueNode : IRunnable
        {
            readonly Action action;

            public ActionTaskQueueNode(Action action)
            {
                this.action = action;
            }

            public void Run() => this.action();
        }

        sealed class StateActionTaskQueueNode : IRunnable
        {
            readonly Action<object> action;
            readonly object state;

            public StateActionTaskQueueNode(Action<object> action, object state)
            {
                this.action = action;
                this.state = state;
            }

            public void Run() => this.action(this.state);
        }

        sealed class StateActionWithContextTaskQueueNode : IRunnable
        {
            readonly Action<object, object> action;
            readonly object context;
            readonly object state;

            public StateActionWithContextTaskQueueNode(Action<object, object> action, object context, object state)
            {
                this.action = action;
                this.context = context;
                this.state = state;
            }

            public void Run() => this.action(this.context, this.state);
        }

        abstract class FuncQueueNodeBase<T> : IRunnable
        {
            readonly TaskCompletionSource<T> promise;
            readonly CancellationToken cancellationToken;

            protected FuncQueueNodeBase(TaskCompletionSource<T> promise, CancellationToken cancellationToken)
            {
                this.promise = promise;
                this.cancellationToken = cancellationToken;
            }

            public Task<T> Completion => this.promise.Task;

            public void Run()
            {
                if (this.cancellationToken.IsCancellationRequested)
                {
                    this.promise.TrySetCanceled();
                    return;
                }

                try
                {
                    T result = this.Call();
                    this.promise.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    // todo: handle fatal
                    this.promise.TrySetException(ex);
                }
            }

            protected abstract T Call();
        }

        sealed class FuncSubmitQueueNode<T> : FuncQueueNodeBase<T>
        {
            readonly Func<T> func;

            public FuncSubmitQueueNode(Func<T> func, CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(), cancellationToken)
            {
                this.func = func;
            }

            protected override T Call() => this.func();
        }

        sealed class StateFuncSubmitQueueNode<T> : FuncQueueNodeBase<T>
        {
            readonly Func<object, T> func;

            public StateFuncSubmitQueueNode(Func<object, T> func, object state, CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(state), cancellationToken)
            {
                this.func = func;
            }

            protected override T Call() => this.func(this.Completion.AsyncState);
        }

        sealed class StateFuncWithContextSubmitQueueNode<T> : FuncQueueNodeBase<T>
        {
            readonly Func<object, object, T> func;
            readonly object context;

            public StateFuncWithContextSubmitQueueNode(Func<object, object, T> func, object context, object state, CancellationToken cancellationToken)
                : base(new TaskCompletionSource<T>(state), cancellationToken)
            {
                this.func = func;
                this.context = context;
            }

            protected override T Call() => this.func(this.context, this.Completion.AsyncState);
        }

        #endregion
    }
}
