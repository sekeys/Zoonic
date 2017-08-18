using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic.Lib.Authentication
{
    public abstract class UserContext
    {
        public abstract IUser GetUser();
        public abstract void SetThreadUser();
        public abstract void Processe();


        public abstract Task<IUser> GetUserAsync();
        public abstract Task SetThreadUserAsync();
        public abstract Task ProcesseAsync();

        public abstract Task SetUserRoleAsync();
        public abstract void SetUserRole();
    }
}
