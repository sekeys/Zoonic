using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Log
{
    public interface ILog
    {
        void Log(LogInformation infos);
    }
}
