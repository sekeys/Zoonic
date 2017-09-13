using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zoonic.Lib;

namespace Zoonic.Web.Route
{
    public class DefaultRoute : RouteBase
    {
        public DefaultRoute()
        {
        }
        public override async Task<IProcessor> RouteAsync()
        {
            return await Task.Run<IProcessor>(() =>
            {
                var url = AccessorContext.DefaultContext.Get<IgnoreDynamic>("parameter").Get<string>("url")
                    .Split('/').Where(m => !string.IsNullOrWhiteSpace(m));
                var first = url == null || url.Count() == 0 ? "" : url.ElementAt(0);
                var proc = DefaultProcessorFactory.Factory.Cache(first);

                return proc;
            });
        }
    }
}
