using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Javascript.Hosting;

namespace Zoonic.Javascript
{
    public class JavascriptWrapper
    {
        private readonly JavaScriptNativeFunction invoke = Invoke;
        public readonly object Value;
        public JavascriptWrapper(object value)
        {
            Value = value;
        }

        public JavaScriptValue Wrap()
        {
            JavaScriptValue jsvalue;
            if (Native.JsCreateObject(out jsvalue) == JavaScriptErrorCode.NoError)
            {
                JavascriptHost.DefineHostCallback("invoke", invoke);
                JavascriptSerializer.Serialize(jsvalue, Value);
                JavascriptSerializer.SerializeMethod(jsvalue, Value);
                return jsvalue;
            }
            return JavaScriptValue.Undefined;
        }
        private static JavaScriptValue Invoke(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            if (arguments.Length < 2)
            {
                return JavaScriptValue.Undefined;
            }
            var methodName = arguments[1];

            return JavaScriptValue.Undefined;
        }

    }
}
