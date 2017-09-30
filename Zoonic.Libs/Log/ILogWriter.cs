using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Log
{
    public interface ILogWriter
    {
        void Write(string info);
        void Write(LogLevel logLevel,string info);
        void Write(string guid,LogLevel logLevel, string info);
        void Write(LogInformation logInformation);
    }
}
