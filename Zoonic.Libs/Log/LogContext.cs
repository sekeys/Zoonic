using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib.Log
{
    public class LogContext : Context, IErrors, IDebug, IException, IWarning,IPackage
    {
        public ILogQueue Queue { get; private set; } = null;
        public void Debug(object obj)
        {
            throw new NotImplementedException();
        }
        public static void EnqueuePackage()
        {

        }

        public void Error(LogInformation info)
        {
            throw new NotImplementedException();
        }

        public void Exception(LogInformation info)
        {
            throw new NotImplementedException();
        }

        public void Warning(LogInformation info)
        {
            throw new NotImplementedException();
        }

        public void Equeue(LogInformation info)
        {
            throw new NotImplementedException();
        }
    }


}
