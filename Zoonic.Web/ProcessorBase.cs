﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zoonic.Lib;
using Zoonic.Lib.Authentication;

namespace Zoonic.Web
{
    public abstract class ProcessorBase:IProcessor
    {
        public HttpContext Context
        {
            get { return Accessor<HttpContext>.Current; }
        }

        public HttpRequest Request { get => Context.Request; }
        public IUser User { get => AccessorContext.DefaultContext.Get<IUser>("user"); }


        public dynamic Parameter { get => AccessorContext.DefaultContext.Get<IgnoreDynamic>("parameter");  }

        public abstract Task<ProcessorResult> Processe();
    }
}
