using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Attributes
{
    public abstract class LogAttribute:Attribute
    {
        public abstract void Log();
    }
}
