using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Log
{
    public interface IErrors
    {
        void Error(LogInformation info);
    }
}
