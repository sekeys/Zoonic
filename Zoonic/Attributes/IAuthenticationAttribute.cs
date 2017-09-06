using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Lib.Authentication;

namespace Zoonic.Attributes
{
    public interface IAuthenticationAttribute
    {
        void Authorized(IUser user);
    }
}
