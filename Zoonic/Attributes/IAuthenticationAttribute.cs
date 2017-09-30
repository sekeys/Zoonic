using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Authentication;

namespace Zoonic.Attributes
{
    public interface IAuthenticationAttribute
    {
        void Authorized(IUser user);
    }
}
