
namespace Zoonic.Concurrency
{
    using System;
    using System.Threading;
    using Zoonic.Lib;

    sealed class ActionScheduledAsyncTask : ScheduledAsyncTask
    {
        readonly Action action;

        public ActionScheduledAsyncTask(Executor executor, Action action, PreciseTimeSpan deadline, CancellationToken cancellationToken)
            : base(executor, deadline, new TaskCompletionSource(), cancellationToken)
        {
            this.action = action;
        }

        protected override void Execute() => this.action();
    }
}
