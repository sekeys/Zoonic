using System;
using Zoonic.Interface;

namespace Zoonic.Concurrency
{
    using System;
    using Zoonic.Lib;

    sealed class StateActionScheduledTask<T> : ScheduledTask<T>
    {
        readonly Action<object> action;

        public StateActionScheduledTask(Executor executor, Action<object> action, object state, PreciseTimeSpan deadline)
            : base(executor, deadline, new TaskCompletionSource(state))
        {
            this.action = action;
        }

        protected override void Execute() => this.action(this.Completion.AsyncState);
    }
}