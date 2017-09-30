
namespace Zoonic.Web
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Zoonic.Concurrency;
    using Zoonic;
    using Zoonic.Authentication;

    public abstract class WebHandler : IHandler
    {
        private HttpContext httpContext;
        protected dynamic Parameter
        {
            get
            {
               return AccessorContext.DefaultContext.Get<IgnoreDynamic>("parameter");
            }
        }
        private IUser user;
        public HttpContext HttpContext => httpContext;
        public IUser User => user;
        public WebHandler()
        {
        }
        public void Handle()
        {

            httpContext = AccessorContext.DefaultContext.Get<HttpContext>();
            user = AccessorContext.DefaultContext.Get<IUser>();
            HandleCore();
        }
        protected abstract void HandleCore();
    }
}
