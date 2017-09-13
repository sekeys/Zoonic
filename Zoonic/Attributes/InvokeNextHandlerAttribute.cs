using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Concurrency;

namespace Zoonic.Attributes
{
    public abstract class InvokeNextHandlerAttribute:Attribute
    {
        public abstract IHandler Invoke();
    }
}
