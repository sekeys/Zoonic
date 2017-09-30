using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Concurrency
{
    using System;
    using System.Threading;
    using Zoonic;

    sealed class StateWithContextScheduledAsyncTask : ScheduledAsyncTask
    {
        readonly Action<object, object> action;
        readonly object context;

        public StateWithContextScheduledAsyncTask(Executor executor, Action<object, object> action, object context, object state,
            PreciseTimeSpan deadline, CancellationToken cancellationToken)
            : base(executor, deadline, new TaskCompletionSource(state), cancellationToken)
        {
            this.action = action;
            this.context = context;
        }

        protected override void Execute() => this.action(this.context, this.Completion.AsyncState);
    }
}
