using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Log
{
    public interface IException
    {
        void Exception(LogInformation info);
    }
}
