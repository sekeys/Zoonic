using System;
using System.Collections.Generic;
using System.Text;
using Zoonic;

namespace Zoonic.Web.Handlers
{
    public class ProcessHandler : WebHandler
    {
        protected override async void HandleCore()
        {
            IProcessor processor = AccessorContext.DefaultContext.Get<IProcessor>();
            var t = processor.Process();
            t.Wait();
            if (t.Result == null)
            {
                await new JsonResult(null).Execute();
            }
            await t.Result.Execute();
        }
    }
}
