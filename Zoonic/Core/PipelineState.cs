using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Concurrency
{
    public enum PipelineState
    {
        Unstarted,
        Running,
        Completed,
        Exception,
        Canceled,
    }
}
