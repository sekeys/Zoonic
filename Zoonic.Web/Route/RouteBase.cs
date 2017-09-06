using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic.Web.Route
{
    public abstract class RouteBase : IRoute
    {
        public UrlTree UrlTree { get { return Zoonic.Web.Route.UrlTree.Tree; } }

        public abstract Task<IProcessor> RouteAsync();
    }
}
