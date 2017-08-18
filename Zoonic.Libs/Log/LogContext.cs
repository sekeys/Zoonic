using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib.Log
{
    public class LogContext : Context, IError, IDebug, IException, IWarning
    {
        public void Debug(object obj)
        {
            throw new NotImplementedException();
        }
    }


}
