using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Zoonic.Javascript.Hosting;

namespace Zoonic.Javascript
{
    public  class JavascriptDelegate
    {
        public JavaScriptNativeFunction Function;
        public object Target { get;protected set; }
        public MethodInfo Method { get; protected set; }
        public ParameterInfo[] Parameter { get { return Method.GetParameters(); } }
        public JavascriptDelegate(object target,MethodInfo methodInfo)
        {
            Function = Invoke;
            Target = target;

        }
        protected object[] InitializeParameter(JavaScriptValue[] arguments)
        {
            ParameterInfo[] paramInfo = this.Parameter;
            int length = paramInfo.Length;
            int arLength = arguments.Length;
            var parameter = new object[length];
            for(var i = 0; i < length; i++)
            {
                var p = paramInfo[i];
                if(i> arLength)
                {
                    parameter[i] = p.DefaultValue;
                }
                else
                {
                    parameter[i] = JavascriptSerializer.Deserialize(Activator.CreateInstance(p.ParameterType), arguments[i]);
                }
            }

            return parameter;
        }
        private JavaScriptValue Invoke(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {

            Method.Invoke(Target, InitializeParameter(arguments));
            return JavaScriptValue.Undefined;
        }
    }
}
