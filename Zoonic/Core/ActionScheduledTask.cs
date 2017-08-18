
namespace Zoonic.Concurrency
{
    using Zoonic.Interface;
    using System;
    using Zoonic.Lib;

    sealed class ActionScheduledTask : ScheduledTask
    {
        readonly Action action;

        public ActionScheduledTask(Executor executor, Action action, PreciseTimeSpan deadline)
            : base(executor, deadline, new TaskCompletionSource())
        {
            this.action = action;
        }

        protected override void Execute() => this.action();
    }
}