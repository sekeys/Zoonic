using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Log
{
    public interface ILogQueue : ILogPush
    {
        void Enqueue();
        LogInformation Dequeue();

        int WaitWriting { get; }
        bool CanPush { get; }
    }
}
