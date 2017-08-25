

namespace Zoonic.Concurrency
{
    using System;
    using System.Threading;
    using Zoonic.Lib;

    sealed class StateActionScheduledAsyncTask<T> : ScheduledAsyncTask
    {
        readonly Action<object> action;

        public StateActionScheduledAsyncTask(Executor executor, Action<object> action, object state, PreciseTimeSpan deadline,
            CancellationToken cancellationToken)
            : base(executor, deadline, new TaskCompletionSource(state), cancellationToken)
        {
            this.action = action;
        }

        protected override void Execute() => this.action(this.Completion.AsyncState);
    }
}
