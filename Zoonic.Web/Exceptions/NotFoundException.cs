using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Web.Exceptions
{
    public class NotFoundException:Exception
    {
        public NotFoundException(string msg) : base( msg)
        {

        }
    }
}
