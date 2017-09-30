

namespace Zoonic.Concurrency
{
    using Zoonic.Interface;
    using System;
    using Zoonic;

    public interface IScheduledRunnable : IRunnable, IScheduledTask, IComparable<IScheduledRunnable>
    {
    }
}