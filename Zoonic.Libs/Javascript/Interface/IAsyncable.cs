using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Javascript.Hosting;

namespace Zoonic.Javascript.Interface
{
    public interface IAsyncable:IJavascriptRegister
    {
        JavaScriptValue Wait(JavaScriptValue timeout);
        void Then(JavaScriptValue callback);
        void Catch(JavaScriptValue callback);
        IAsyncableResult Result();
    }
}
