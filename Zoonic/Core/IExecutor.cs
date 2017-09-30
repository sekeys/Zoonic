

namespace Zoonic.Concurrency
{
    using Zoonic.Interface;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Zoonic;

    public interface IExecutor
    {
        bool InLoop { get; }
        
        bool IsShuttingDown { get; }
        
        Task TerminationCompletion { get; }
        
        bool IsShutdown { get; }
        
        bool IsTerminated { get; }

        /// <summary>
        /// Parent <see cref="IEventExecutorGroup"/>.
        /// </summary>
        IExecutorGroup Parent { get; }
        
        bool IsInLoop(Thread thread);
        
        void Execute(IRunnable task);
        
        void Execute(Action<object> action, object state);
        
        void Execute(Action action);
        
        void Execute(Action<object, object> action, object context, object state);
        
        IScheduledTask Schedule(IRunnable action, TimeSpan delay);
        
        IScheduledTask Schedule(Action action, TimeSpan delay);
        
        IScheduledTask Schedule(Action<object> action, object state, TimeSpan delay);
        
        IScheduledTask Schedule(Action<object, object> action, object context, object state, TimeSpan delay);
        
        Task ScheduleAsync(Action<object> action, object state, TimeSpan delay, CancellationToken cancellationToken);
        
        Task ScheduleAsync(Action<object> action, object state, TimeSpan delay);
        
        Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken cancellationToken);
        
        Task ScheduleAsync(Action action, TimeSpan delay);
        
        Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay);
        
        Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay, CancellationToken cancellationToken);

        
        Task<T> SubmitAsync<T>(Func<T> func);
        
        Task<T> SubmitAsync<T>(Func<T> func, CancellationToken cancellationToken);
        
        Task<T> SubmitAsync<T>(Func<object, T> func, object state);
        
        Task<T> SubmitAsync<T>(Func<object, T> func, object state, CancellationToken cancellationToken);
        
        Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state);
        
        Task<T> SubmitAsync<T>(Func<object, object, T> func, object context, object state, CancellationToken cancellationToken);
        
        Task ShutdownGracefullyAsync();
        
        Task ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout);
    }
}
