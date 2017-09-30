using System;
using System.Collections.Generic;
using System.Text;
using Zoonic;
using Zoonic.Web.Exceptions;
using Zoonic.Web.Route;

namespace Zoonic.Web
{
    public class RouteHandler : WebHandler
    {
        protected override void HandleCore()
        {
            RouteValueDictionary routeData = new RouteValueDictionary();
            var router = UrlTree.Tree.Match(AccessorContext.DefaultContext.Get<IgnoreDynamic>("parameter").Get<string>("url"), HttpContext, routeData);
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
