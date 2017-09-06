using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Web.Exceptions
{
    public class BaseException:System.Exception
    {
        public int Status { get; private set; } = 500;
        public BaseException()
        {

        }
        public BaseException(string message,int status=500):base(message)
        {

        }
        public BaseException(string message,Exception exception) : base(message,exception)
        {

        }
    }
}
