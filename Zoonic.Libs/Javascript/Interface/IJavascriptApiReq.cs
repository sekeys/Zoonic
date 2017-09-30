using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Javascript.Interface
{
    public interface IJavascriptApiReq : IJavascriptRegister
    {
        Asyncable Request(string data, string method, KeyValuePair<string, string> header,string encoding);
        Asyncable Request(string data, string method, KeyValuePair<string, string> header, Encoding encoding);
        Asyncable Request(string data, string method, KeyValuePair<string, string> header);
        Asyncable Request(string data, string method);
        Asyncable Request(string data, string method, string encoding);
        Asyncable Request(string data, string method, Encoding encoding);
        Asyncable Request(string data);
    }
}
