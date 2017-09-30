using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Zoonic.Javascript.Hosting;
using Zoonic.Javascript.Interface;

namespace Zoonic.Javascript
{
    public class DefaultJavascriptApiReq : IJavascriptApiReq
    {
        public DefaultJavascriptApiReq()
        {
            Conf = new ApiConf()
            {
                Url = "",
                Method = "get",
                Encoding = "utf-8"
            };
        }
        public struct ApiConf
        {
            public string Key { get; set; }
            public string Url { get; set; }

            public string Method { get; set; }

            public Dictionary<string, string> Header { get; set; }

            public string Encoding { get; set; }
            public void Merge(JavaScriptValue javaScript)
            {
                var url = javaScript.GetProperty(JavaScriptPropertyId.FromString("url"));
                if(url.IsValid && url.ValueType== JavaScriptValueType.String)
                {
                    this.Url = url.ConvertToString().ToString();
                }


                var method = javaScript.GetProperty(JavaScriptPropertyId.FromString("method"));
                if (method.IsValid && method.ValueType == JavaScriptValueType.String)
                {
                    this.Method = method.ConvertToString().ToString();
                }


                var encoding = javaScript.GetProperty(JavaScriptPropertyId.FromString("encoding"));
                if (encoding.IsValid && encoding.ValueType == JavaScriptValueType.String)
                {
                    this.Encoding = encoding.ConvertToString().ToString();
                }

                var header = javaScript.GetProperty(JavaScriptPropertyId.FromString("header"));
                if(header.IsValid && header.ValueType== JavaScriptValueType.Array)
                {
                    var jsIndexProperty = header.GetOwnPropertyNames();

                }
            }

            public void Merge(ApiConf conf)
            {
                if (!string.IsNullOrWhiteSpace(conf.Method))
                {
                    this.Method = conf.Method;
                }
                if (!string.IsNullOrWhiteSpace(conf.Encoding))
                {
                    this.Encoding = conf.Encoding;
                }
                if (!string.IsNullOrWhiteSpace(conf.Url))
                {
                    this.Url = conf.Url;
                }
                if (Header!=null)
                {
                    if (this.Header == null)
                    {
                        this.Header = new Dictionary < string, string>();
                    }
                    foreach(var item in this.Header)
                    {
                        if (this.Header.ContainsKey(item.Key))
                        {
                            this.Header[item.Key] = item.Value;
                        }
                        else
                        {
                            this.Header.Add(item.Key,item.Value);
                        }
                    }
                }
            }

        }
        public Asyncable Request(string data)
        {
            return DefaultJavascriptApiReq.Request(this.Conf.Url, data, this.Conf.Method, this.Conf.Header, this.Conf.Encoding);
        }
        protected virtual JavaScriptValue JavascriptRequest(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData)
        {
            if (argumentCount == 1)
            {
                return this.Request("").Register();
            }
            var data = arguments[1].ConvertToString().ToString();
            if (argumentCount == 3)
            {
                Conf.Merge(arguments[2]);
            }
            return this.Request(data).Register();
        }
        public ApiConf Conf { get; set; }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
       

        public static Asyncable Request(string url, string data, string method, IEnumerable<KeyValuePair<string, string>> header)
        {
            throw new NotImplementedException();
        }

        public static Asyncable Request(string url, string data, string method) => Request(url, data, method, Encoding.GetEncoding("utf-8"));

        public static Asyncable Request(string url, string data) => Request(url, data, "get");

        public static Asyncable Request(string url, string data, string method, IEnumerable<KeyValuePair<string, string>> header, string encoding) => Request(url, data, method, header, Encoding.GetEncoding(encoding));

        public static Asyncable Request(string url, string data, string method, IEnumerable<KeyValuePair<string, string>> header, Encoding encoding)
        {
            return new Asyncable(
                Task.Run<JavaScriptValue>(() =>
                {
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
                    if (header != null)
                    {
                        foreach (var item in header)
                        {
                            httpRequest.Headers.Add(item.Key, item.Value);
                        }
                    }
                    if (0 < data.Length)
                    {
                        byte[] datas = Encoding.UTF8.GetBytes(data);
                        using (Stream stream = httpRequest.GetRequestStream())
                        {
                            stream.Write(datas, 0, data.Length);
                        }
                    }
                    try
                    {
                        httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    }
                    catch (WebException ex)
                    {
                        throw ex;
                    }
                    Stream st = httpResponse.GetResponseStream();
                    StreamReader reader = new StreamReader(st, encoding);
                    var resultString = reader.ReadToEnd();

                    if (httpResponse.ContentType.Contains("json"))
                    {
                        return JavaScriptValue.FromObject(Newtonsoft.Json.JsonConvert.DeserializeObject(resultString));
                    }
                    else
                    {
                        return JavaScriptValue.FromString(resultString);
                    }
                })
            );
        }

        public static Asyncable Request(string url, string data, string method, string encoding) => Request(url, data, method, Encoding.GetEncoding(encoding));

        public static Asyncable Request(string url, string data, string method, Encoding encoding) => Request(url, data, method, null, encoding);
        
        public static Asyncable Request(string url, Encoding encoding) => Request(url, "", "get", encoding);

        public static Asyncable Request(string url, IEnumerable<KeyValuePair<string, string>> header, Encoding encoding) => Request(url, "", "get", encoding);

        public static Asyncable Request(string url, IEnumerable<KeyValuePair<string, string>> header, string encoding) => Request(url, "", "get", encoding);

        


    }
}
