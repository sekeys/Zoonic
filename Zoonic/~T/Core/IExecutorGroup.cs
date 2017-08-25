using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zoonic.Concurrency;

namespace Zoonic.Concurrency
{
    public interface IExecutorGroup<T>
    {
        Task<T> TerminationCompletion { get; }

        IExecutor<T> GetNext();

        Task<T> ShutdownGracefullyAsync();

        Task<T> ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout);
    }
}
