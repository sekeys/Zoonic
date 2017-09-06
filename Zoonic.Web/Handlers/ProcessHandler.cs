using System;
using System.Collections.Generic;
using System.Text;
using Zoonic.Lib;

namespace Zoonic.Web.Handlers
{
    public class ProcessHandler : WebHandler
    {
        protected override async void HandleCore()
        {
            IProcessor processor = AccessorContext.DefaultContext.Get<IProcessor>();
            var t = processor.Processe();
            t.Wait();
            if (t.Result == null)
            {
                await new JsonResult(null).Execute();
            }
            await t.Result.Execute();
        }
    }
}
