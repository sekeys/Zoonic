﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zoonic;

namespace Zoonic.Web
{
    public abstract class ProcessorResult
    {
        public HttpContext HttpContext
        {
            get { return AccessorContext.DefaultContext.Get<HttpContext>(); }
        }

        public HttpRequest Request { get => HttpContext.Request; }
        public HttpResponse Response { get => HttpContext.Response; }
        public abstract Task Execute();
    }
}
