using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Javascript.Hosting;

namespace Zoonic.Javascript.Interface
{
    public interface IJavascriptSchedule<T>:IAsyncable
    {
        void Schedule(params IJavascriptCallback[] javaScriptValue);

    }
}
