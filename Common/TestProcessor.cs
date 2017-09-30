using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zoonic.Web;

namespace Common
{
    public class TestProcessor : ProcessorBase
    {
        public override async Task<ProcessorResult> Process()
        {
            return new JsonResult( new
            {
                result = true,
                data = ""
            });
        }
    }
}
