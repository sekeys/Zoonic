using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Concurrency
{
    using Zoonic.Interface;
    using System;
    using Zoonic;
    #region 
    internal sealed class RunnableNode : IRunnable
    {
        public RunnableNode(Action action)
        {
            this.action = action;
        }
        readonly Action action;
        public void Run()
        {
            action();
        }
    }
    internal sealed class StateRunnableNode : IRunnable
    {
        readonly object State;
        public StateRunnableNode(Action<object> action, object state)
        {
            this.action = action;
            State = state;
        }
        readonly Action<object> action;
        public void Run()
        {
            action(State);
        }
    }
    internal sealed class StateWithContextRunnableNode : IRunnable
    {
        readonly object State;
        readonly object Context;
        public StateWithContextRunnableNode(Action<object, object> action, object context, object state)
        {
            this.action = action;
            State = state;
            Context = context;
        }
        readonly Action<object, object> action;
        public void Run()
        {
            action(Context, State);
        }
    }
    #endregion
}
