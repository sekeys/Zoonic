

namespace Zoonic.Concurrency
{
    using Zoonic.Interface;
    using System;
    using Zoonic.Lib;

    public interface IScheduledRunnable : IRunnable, IScheduledTask, IComparable<IScheduledRunnable>
    {
    }
}