﻿using System;
using Zoonic.Interface;

namespace Zoonic.Concurrency
{
    using System;
    using Zoonic.Lib;

    sealed class StateActionWithContextScheduledTask<T> : ScheduledTask
    {
        readonly Action<object, object> action;
        readonly object context;

        public StateActionWithContextScheduledTask(Executor executor, Action<object, object> action, object context, object state,
            PreciseTimeSpan deadline)
            : base(executor, deadline, new TaskCompletionSource(state))
        {
            this.action = action;
            this.context = context;
        }

        protected override void Execute() => this.action(this.context, this.Completion.AsyncState);
    }
}