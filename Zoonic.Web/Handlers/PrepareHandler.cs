using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Concurrency;
using Zoonic.Configuration;
using Zoonic.Lib;

namespace Zoonic.Web
{
    public class PrepareHandler : WebHandler
    {
        protected override void HandleCore()
        {
            string url = HttpContext.Request.Path.Value;
            url = url.Trim('/');
            if (string.IsNullOrWhiteSpace(url))
            {
                url = Appsetting.AppSettings.String("default.path");
            }
            if (string.IsNullOrWhiteSpace(url))
            {
                url = "/test";
            }
            dynamic dynamic = new IgnoreDynamic();
            dynamic.url = url;
            dynamic.httpMethod = HttpContext.Request.Method;
            AccessorContext.DefaultContext.Set<IgnoreDynamic>("parameter", dynamic);
        }
    }
}
