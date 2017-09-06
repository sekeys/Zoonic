using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Lib;
using Zoonic.Web.Exceptions;
using Zoonic.Web.Route;

namespace Zoonic.Web
{
    public class RouteHandler : WebHandler
    {
        protected override void HandleCore()
        {
            RouteValueDictionary routeData = new RouteValueDictionary();
            var router = UrlTree.Tree.Match(HttpContext.Request.Path,HttpContext, routeData);
            var t= router.RouteAsync();
            t.Wait();
            AccessorContext.DefaultContext.Set<RouteValueDictionary>(routeData);
            if (t.Result==null)
            {
                throw new NotFoundException("un found the special processor");
            }
            AccessorContext.DefaultContext.Set<IProcessor>(t.Result);
        }
    }
}
