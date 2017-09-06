using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Web.Exceptions
{
    public class AuthenticationFailedException:StatusException
    {
        public AuthenticationFailedException(string message):base(message,StatusCode.UNAUTHENTENICATION)
        {

        }
    }
}
