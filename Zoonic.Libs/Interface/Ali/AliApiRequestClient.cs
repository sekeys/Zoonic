using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text;
using System.IO;

namespace Zoonic.Interface.Ali
{
    public class AliApiRequestClient
    {
        //using System.IO;
        //using System.Text;
        //using System.Net;

        //private const String host = "http://jisutqybmf.market.alicloudapi.com";
        //private const String path = "/weather/query";
        //private const String method = "GET";
        //private const String appcode = "你自己的AppCode";

        public static string Request(string url, string appcode, string bodys = "", string method = "GET")
        {
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
            httpRequest.Headers.Add("Authorization", "APPCODE " + appcode);
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

            Console.WriteLine(httpResponse.StatusCode);
            Console.WriteLine(httpResponse.Method);
            Console.WriteLine(httpResponse.Headers);
            Stream st = httpResponse.GetResponseStream();
            StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
            return reader.ReadToEnd();

        }

        public static string Request(string url, string query, string appcode, string bodys = "", string method = "GET")
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                if (url.IndexOf('?') > 0)
                {
                    url += "&" + query;
                }
                else
                {
                    url = $"{url}?{query}";
                }
            }
            return Request(url, appcode, bodys, method);
        }
        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}