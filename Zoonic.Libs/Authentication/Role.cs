using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Authentication
{
    public class Role
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int Level { get; set; }
        public Role Parent { get; set; }

        public IList<Role> Children { get; set; }

    }
}
