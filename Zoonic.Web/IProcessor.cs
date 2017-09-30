using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic.Web
{
    public interface IProcessor
    {

        Task<ProcessorResult> Process();
    }
}
