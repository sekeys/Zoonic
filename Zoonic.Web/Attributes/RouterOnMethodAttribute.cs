using Zoonic.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Web.Attributes
{
    public class RouterOnMethodAttribute:Attribute
    {
        public string Method { get; private set; }
        public RouterOnMethodAttribute(string method)
        {
            Method = method;
        }
    }
}
