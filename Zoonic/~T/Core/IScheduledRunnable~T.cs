

namespace Zoonic.Concurrency
{
    using Zoonic.Interface;
    using System;
    using Zoonic.Lib;

    public interface IScheduledRunnable<T> : IRunnable<T>, IScheduledTask<T>, IComparable<IScheduledRunnable<T>>
    {
    }
}