using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib.Log
{
    public interface ILogPush
    {
        ILogWriter Writer { get; }
        void Push();
    }
}
