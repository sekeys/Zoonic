using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Javascript.Hosting;
using Zoonic.Javascript.Interface;

namespace Zoonic.Javascript
{
    public class DefaultJavascriptApiReq : IJavascriptApiReq, IJavascriptRegister
    {
        public JavaScriptValue Register()
        {
            throw new NotImplementedException();
        }

        public Asyncable Request(string data, string method, KeyValuePair<string, string> header)
        {
            throw new NotImplementedException();
        }

        public Asyncable Request(string data, string method)=>Request(data,method,Encoding.GetEncoding("utf-8"));

        public Asyncable Request(string data) => Request(data, "get");

        public Asyncable Request(string data, string method, KeyValuePair<string, string> header, string encoding)
        {
            throw new NotImplementedException();
        }

        public Asyncable Request(string data, string method, KeyValuePair<string, string> header, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public Asyncable Request(string data, string method, string encoding)
        {
            throw new NotImplementedException();
        }

        public Asyncable Request(string data, string method, Encoding encoding)
        {
            throw new NotImplementedException();
        }
    }
}
