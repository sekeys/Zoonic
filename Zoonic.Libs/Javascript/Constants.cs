
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Zoonic.Configuration;
using Zoonic.Javascript.Hosting;

namespace Zoonic.Javascript
{
    public class Constants
    {
        private static JavaScriptSourceContext currentSourceContext = JavaScriptSourceContext.FromIntPtr(IntPtr.Zero);


        private static readonly JavaScriptNativeFunction runScriptDelegate = RunScript;
        private static readonly JavaScriptNativeFunction requestDataDelegate = RequestData;
        private static readonly JavaScriptNativeFunction getSettingDelegate = GetSetting;
        private static readonly JavaScriptNativeFunction getWeixinTimeDelegate = GetWeixinDatetime;
        public readonly static DateTime BaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//Unix起始时间
        /// <summary>
        /// 获取微信DateTime（UNIX时间戳）
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <returns></returns>
        public static long GetWeixinDateTime(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - BaseTime).TotalSeconds;
        }

        private static JavaScriptValue RunScript(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            if (argumentCount < 2)
            {
                return JavaScriptValue.Invalid;
            }

            //
            // Convert filename.
            //

            string filename = arguments[1].ToString();

            //
            // Load the script from the disk.
            //

            string script = File.ReadAllText(filename);
            if (string.IsNullOrEmpty(script))
            {
                return JavaScriptValue.Invalid;
            }

            //
            // Run the script.
            //
            return JavaScriptContext.RunScript(script, currentSourceContext++, filename);
        }
        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        private static JavaScriptValue RequestData(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            try
            {
                var jsParameter = arguments[1];
                //var jnames = jsParameter.GetOwnPropertyNames();
                string url = jsParameter.GetProperty(JavaScriptPropertyId.FromString("url")).ConvertToString().ToString();
                string method = jsParameter.GetProperty(JavaScriptPropertyId.FromString("method")).ConvertToString().ToString();
                var bodysJPro = jsParameter.GetProperty(JavaScriptPropertyId.FromString("body"));
                string bodys = "";
                if (bodysJPro.ValueType != JavaScriptValueType.Null && bodysJPro.ValueType == JavaScriptValueType.Undefined)
                {
                    bodys = "";
                }
                else
                {
                    bodys = bodysJPro.ConvertToString().ToString();
                }
                //String querys = "city=%E5%AE%89%E9%A1%BA&citycode=citycode&cityid=cityid&ip=ip&location=location";
                HttpWebRequest httpRequest = null;
                HttpWebResponse httpResponse = null;

                if (url.Contains("https://"))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    httpRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                }
                else
                {
                    httpRequest = (HttpWebRequest)WebRequest.Create(url);
                }
                httpRequest.Method = method;

                var headersJPro = jsParameter.GetProperty(JavaScriptPropertyId.FromString("header"));
                if (headersJPro.ValueType != JavaScriptValueType.Undefined && headersJPro.ValueType != JavaScriptValueType.Null)
                {
                    var lengthJPro = headersJPro.GetProperty(JavaScriptPropertyId.FromString("length"));
                    if (lengthJPro.IsValid)
                    {
                        var length = lengthJPro.ToInt32();
                        for (var i = 0; i < length; i++)
                        {
                            var itemJpro = headersJPro.GetProperty(JavaScriptPropertyId.FromString(i.ToString()));
                            if (itemJpro.IsValid)
                            {
                                var itemKeyJPro = itemJpro.GetProperty(JavaScriptPropertyId.FromString("key"));
                                var itemValueJPro = itemJpro.GetProperty(JavaScriptPropertyId.FromString("value"));
                                httpRequest.Headers.Add(itemKeyJPro.ToString(), itemValueJPro.ToString());
                            }
                        }
                    }
                }
                //httpRequest.Headers.Add("Authorization", "APPCODE " + appcode);
                if (0 < bodys.Length)
                {
                    byte[] data = Encoding.UTF8.GetBytes(bodys);
                    using (Stream stream = httpRequest.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                try
                {
                    httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                }
                catch (WebException ex)
                {
                    httpResponse = (HttpWebResponse)ex.Response;
                }
                Stream st = httpResponse.GetResponseStream();
                StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
                string str = reader.ReadToEnd();

                JavaScriptValue response = JavaScriptValue.CreateObject();
                response.SetProperty(JavaScriptPropertyId.FromString("status"), JavaScriptValue.FromInt32(200), true);

                response.SetProperty(JavaScriptPropertyId.FromString("response"), JavaScriptValue.FromString(str), true);
                //var js= JavaScriptValue.FromObject(new
                //{
                //    response = str,
                //    status = 200
                //});
                return response;
            }
            catch (Exception ex)
            {
                JavaScriptValue response = JavaScriptValue.CreateObject();
                response.SetProperty(JavaScriptPropertyId.FromString("status"), JavaScriptValue.FromInt32(500), true);

                response.SetProperty(JavaScriptPropertyId.FromString("response"), JavaScriptValue.FromString(ex.Message), true);
                response.SetProperty(JavaScriptPropertyId.FromString("stackTrace"), JavaScriptValue.FromString(ex.StackTrace), true);
                return response;
            }
        }
        private static JavaScriptValue GetSetting(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            var setting = arguments[1].ConvertToString().ToString();

            return JavaScriptValue.FromString(Appsetting.AppSettings[setting].ToString());
        }
        private static JavaScriptValue GetWeixinDatetime(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {

            return JavaScriptValue.FromString(GetWeixinDateTime(DateTime.Now).ToString());
        }
        private static void DefineHostCallback(JavaScriptValue globalObject, string callbackName, JavaScriptNativeFunction callback, IntPtr callbackData)
        {
            JavaScriptPropertyId propertyId = JavaScriptPropertyId.FromString(callbackName);

            JavaScriptValue function = JavaScriptValue.CreateFunction(callback, callbackData);

            globalObject.SetProperty(propertyId, function, true);
        }
        private static JavaScriptValue CompareDate(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            var firstDate = arguments[1];
            var secondDate = arguments[2];
            //var jnames = jsParameter.GetOwnPropertyNames();
            var fDate = Convert.ToDateTime(firstDate.ConvertToString().ToString());
            var sDate = Convert.ToDateTime(secondDate.ConvertToString().ToString());
            return JavaScriptValue.FromBoolean((fDate - sDate).Ticks > 0);

        }
        private static JavaScriptValue DiffDate(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            var firstDate = arguments[1];
            var secondDate = arguments[2];
            //var jnames = jsParameter.GetOwnPropertyNames();
            var fDate = Convert.ToDateTime(firstDate.ConvertToString().ToString());
            var sDate = Convert.ToDateTime(secondDate.ConvertToString().ToString());
            return JavaScriptValue.From(fDate - sDate);

        }
        private static JavaScriptValue DiffDateTick(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            var firstDate = arguments[1];
            var secondDate = arguments[2];
            //var jnames = jsParameter.GetOwnPropertyNames();
            var fDate = Convert.ToDateTime(firstDate.ConvertToString().ToString());
            var sDate = Convert.ToDateTime(secondDate.ConvertToString().ToString());
            return JavaScriptValue.From((fDate - sDate).Ticks);

        }
        private static JavaScriptValue ReadFile(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            var path = arguments[1].ConvertToString().ToString();
            var content = File.ReadAllText(path);
            return JavaScriptValue.FromString(content);
        }
        private static JavaScriptValue Log(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            Task.Run(() =>
            {
               
            });
            return JavaScriptValue.Undefined;
        }
        private static JavaScriptValue Assert(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            
            return JavaScriptValue.Undefined;
        }
        private static JavaScriptValue Error(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            Task.Run(() =>
            {

            });
            return JavaScriptValue.Undefined;
        }
        private static JavaScriptValue Trigger(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            Task.Run(() =>
            {

            });
            return JavaScriptValue.Undefined;
        }
        private static JavaScriptValue Broadcast(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            Task.Run(() =>
            {

            });
            return JavaScriptValue.Undefined;
        }
        private static JavaScriptValue Async(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            Task.Run(() =>
            {

            });
            return JavaScriptValue.Undefined;
        }
        private static JavaScriptValue Sync(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            Task.Run(() =>
            {

            });
            return JavaScriptValue.Undefined;
        }
        public static void InitProperty(JavaScriptValue javaScriptValue)
        {
            DefineHostCallback(javaScriptValue, "loadScript", runScriptDelegate, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "request", requestDataDelegate, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "getSetting", getSettingDelegate, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "getWeixinTick", getWeixinTimeDelegate, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "compareDate", CompareDate, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "read", ReadFile, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "diffDate", DiffDate, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "diffDateTick", DiffDateTick, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "log", Log, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "assert", Assert, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "error", Error, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "trigger", Trigger, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "broadcast", Broadcast, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "async", Async, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "sync", Sync, IntPtr.Zero);
            DefineHostCallback(javaScriptValue, "settimeout", SetTimeout.SetTimeoutJavaScriptNativeFunction, IntPtr.Zero);
        }
    }
}
