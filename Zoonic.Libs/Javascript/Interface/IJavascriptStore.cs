using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Javascript.Hosting;

namespace Zoonic.Javascript.Interface
{
    public interface IJavascriptStore
    {
        JavaScriptValue Read(JavaScriptValue read);
        JavaScriptValue Write(JavaScriptValue write);

        JavaScriptValue Filter(JavaScriptValue filter);

        JavaScriptValue Filter(JavaScriptValue read,JavaScriptValue filter);

        JavaScriptValue Eval();
        void Connect(JavaScriptValue setting);
    }
}
