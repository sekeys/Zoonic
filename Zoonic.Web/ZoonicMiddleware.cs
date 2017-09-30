using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zoonic.Concurrency;
using Zoonic;

namespace Zoonic.Web
{
    public class ZoonicMiddleware
    {
        private RequestDelegate _next;
        public ZoonicMiddleware(RequestDelegate next,IPipeline pipeline)
        {
            Thread.CurrentThread.Name = "Zoonic.Master";
            this.pipeline = pipeline;
            _next = next;

        }
        IPipeline pipeline;

        public async Task Invoke(HttpContext context)
        {
            await Task.Run(() =>
            {
                try
                {
                    AccessorContext.DefaultContext.Set<HttpContext>(context);
                    pipeline.Start();
                    pipeline.Wait();
                }
                catch (Exception ex)
                {
                    context.Response.WriteAsync(ex.ToString());
                }
            });
            
        }

       
    }
}
