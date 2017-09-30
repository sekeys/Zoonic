using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Configuration;
using Zoonic.Javascript.Hosting;

namespace Zoonic.Javascript
{
    public static class AppsettingJsReader
    {

        private static readonly JavaScriptNativeFunction getSettingDelegate = GetSetting;
        private static JavaScriptValue GetSetting(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            var setting = arguments[1].ConvertToString().ToString();
            return JavaScriptValue.FromString(Appsetting.AppSettings[setting].ToString());
        }

        private static readonly JavaScriptNativeFunction setSettingDelegate = SetSetting;
        private static JavaScriptValue SetSetting(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            var setting = arguments[1].ConvertToString().ToString();
            var value = arguments[2].ConvertToString().ToString();
            Appsetting.AppSettings["setting"] = value;
            return arguments[0];
        }

        private static readonly JavaScriptNativeFunction jsonSettingDelegate = Json;
        private static JavaScriptValue Json(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {

            return arguments[0];
        }

        private static readonly JavaScriptNativeFunction fileSettingDelegate = File;
        private static JavaScriptValue File(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            var path = arguments[1].ConvertToString().ToString();
            var priority = arguments.Length == 3 && arguments[2].IsValid && arguments[2].ValueType == JavaScriptValueType.Number
                ? arguments[2].ConvertToNumber().ToInt32() : 99;
            Configuration.ConfigurationManager.Manager.AddFile(path, priority);
            return arguments[0] ;
        }

        public static JavascriptHost RegisterAppsettingModule(this JavascriptHost host,JavaScriptValue jsValue)
        {
            var conf = JavaScriptValue.CreateObject();
            JavascriptHost.DefineHostCallback(conf, "file", fileSettingDelegate, IntPtr.Zero);
            JavascriptHost.DefineHostCallback(conf, "json", jsonSettingDelegate, IntPtr.Zero);
            JavascriptHost.DefineHostCallback(conf, "set", setSettingDelegate, IntPtr.Zero);
            JavascriptHost.DefineHostCallback(conf, "get", getSettingDelegate, IntPtr.Zero);
            JavascriptHost.DefineHostProperty(jsValue, "conf", conf);
            return host;
        }
    }
}
