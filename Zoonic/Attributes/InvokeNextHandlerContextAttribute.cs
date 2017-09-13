using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Concurrency;

namespace Zoonic.Attributes
{
    public abstract class InvokeNextHandlerContextAttribute:Attribute
    {
        public abstract IHandlerContext Invoke();
    }
}
