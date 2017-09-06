using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic.Web.Route
{
    public interface IRoute
    {
        UrlTree UrlTree { get; }

        Task<IProcessor> RouteAsync();

    }
}
