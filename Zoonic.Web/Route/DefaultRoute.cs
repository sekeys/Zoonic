using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic.Web.Route
{
    public class DefaultRoute : RouteBase
    {
        public override Task<IProcessor> RouteAsync()
        {
            return null;
        }
    }
}
