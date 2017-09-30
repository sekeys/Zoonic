using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Javascript.Interface
{
    public interface IJavascriptApiReq
    {
        Asyncable Request(string data);
    }
}
