using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib.Authentication
{
    public interface  IUser
    {
        IList<Role> Roles { get; }
        string Name { get;}
        string Id { get; }
        string Phone { get; }
        string Token { get; }
        
        dynamic Tag { get; }

    }
}
