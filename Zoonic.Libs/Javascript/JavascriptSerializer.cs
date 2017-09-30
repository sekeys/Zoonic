using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Javascript.Hosting;

namespace Zoonic.Javascript
{
    public class JavascriptSerializer
    {
        public static JavaScriptValue Serialize(JavaScriptValue jsValue,object value)
        {
            var type = value.GetType();
            var ps = type.GetProperties();
            foreach (var p in ps)
            {
                if (p.GetMethod != null && p.GetMethod.IsPublic)
                {
                    var v = p.GetValue(value);
                    if (v == null)
                    {
                        continue;
                    }
                    jsValue.SetProperty(
                        JavaScriptPropertyId.FromString(ToLowerFirstLetter(p.Name))
                        , JavaScriptValue.FromString(v.ToString())
                        , true);

                }
            }
            return jsValue;
        }
        private static JavaScriptNativeFunction InvokeMethod;
        public static JavaScriptValue SerializeMethod(JavaScriptValue jsValue, object value)
        {
            var type = value.GetType();
            var ps = type.GetMethods();
            foreach (var p in ps)
            {
                if (p.IsPublic)
                {
                    JavaScriptNativeFunction invokeMethod = (JavaScriptNativeFunction)
                        JavaScriptNativeFunction.CreateDelegate(type, value, p.Name, true, true);
                    JavascriptHost.DefineHostCallback(jsValue, ToLowerFirstLetter(p.Name)
                        , invokeMethod, IntPtr.Zero);
                }
            }
            return jsValue;
        }

        public static object Deserialize(object v, JavaScriptValue javaScriptValue)
        {
            throw new NotImplementedException();
        }

        public static JavaScriptValue Serialize(object value)
        {
            JavaScriptValue jsvalue = JavaScriptValue.CreateObject();
            var type = value.GetType();
            var ps = type.GetProperties();
            foreach (var p in ps)
            {
                if (p.GetMethod != null && p.GetMethod.IsPublic)
                {
                    var v = p.GetValue(value);
                    if (v == null)
                    {
                        continue;
                    }
                    jsvalue.SetProperty(
                        JavaScriptPropertyId.FromString(ToLowerFirstLetter(p.Name))
                        , JavaScriptValue.FromString(v.ToString())
                        , true);

                }
            }
            return jsvalue;
        }
        public static string ToLowerFirstLetter(string field)
        {
            return field[0].ToString().ToLower() + field.PadRight(1);
        }
    }
}
