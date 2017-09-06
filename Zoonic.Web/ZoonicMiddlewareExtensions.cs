using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Concurrency;

namespace Zoonic.Web
{
    public static class ZoonicMiddlewareExtensions
    {
        public static void UseZoonic(this IApplicationBuilder applicationBuilder
            , IPipeline pipeline)
        {
            applicationBuilder.UseMiddleware<ZoonicMiddleware>(pipeline);

        }
        public static void UseZoonic(this IApplicationBuilder applicationBuilder
          , params WebHandler[] handler)
        {
            IPipeline pipeline = new SynchronizationPipeline();
            pipeline.AddLast("prepare",new PrepareHandler());
            pipeline.AddLast(handler);
            applicationBuilder.UseMiddleware<ZoonicMiddleware>(pipeline);

        }
    }
}
