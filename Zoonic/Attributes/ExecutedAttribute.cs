using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Attributes
{
    public abstract class ExecutedAttribute : Attribute
    {
        public int Priority { get; protected set; } = 0;
        public abstract void Execute();
    }
}
