using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Javascript.Hosting;

namespace Zoonic.Javascript.Interface
{
    public interface IAsyncableResult:IAsyncable,IJavascriptRegister
    {
        int Status { get;  }
        string Message { get; }
    }
}
