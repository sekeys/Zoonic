
namespace Zoonic.Concurrency
{
    using System;
    using System.Threading;
    using Zoonic.Lib;

    sealed class FuncScheduledAsyncTask<T> : ScheduledAsyncTask<T>
    {
        readonly Action action;

        public FuncScheduledAsyncTask(Executor executor, Action action, PreciseTimeSpan deadline, CancellationToken cancellationToken)
            : base(executor, deadline, new TaskCompletionSource(), cancellationToken)
        {
            this.action = action;
        }

        protected override void Execute() => this.action();
    }
}
