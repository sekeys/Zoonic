

namespace Zoonic.Concurrency
{
    using System;
    using System.Threading.Tasks;
    public interface IExecutorGroup
    {
        Task TerminationCompletion { get; }
        
        IExecutor GetNext();
        
        Task ShutdownGracefullyAsync();
        
        Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout);
    }
}