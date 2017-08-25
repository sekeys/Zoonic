using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Lib.Authentication;

namespace Zoonic.Attributes
{
    public interface IRoleAttribute
    {
        RoleAuthenticationType Match(IUser user);
    }
}
