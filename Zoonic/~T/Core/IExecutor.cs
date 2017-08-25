using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zoonic.Lib;

namespace Zoonic.Concurrency
{
    public interface IExecutor<T>
    {
        bool InLoop { get; }

        bool IsShuttingDown { get; }

        Task<T> TerminationCompletion { get; }

        bool IsShutdown { get; }

        bool IsTerminated { get; }

        /// <summary>
        /// Parent <see cref="IEventExecutorGroup"/>.
        /// </summary>
        IExecutorGroup Parent { get; }

        bool IsInLoop(Thread thread);

        void Execute(IRunnable<T> task);

        void Execute(Action<object> action, object state);

        void Execute(Action action);

        void Execute(Action<object, object> action, object context, object state);

        IScheduledTask<T> Schedule(IRunnable action, TimeSpan delay);

        IScheduledTask<T> Schedule(Action action, TimeSpan delay);

        IScheduledTask<T> Schedule(Action<object> action, object state, TimeSpan delay);

        IScheduledTask<T> Schedule(Action<object, object> action, object context, object state, TimeSpan delay);

        Task ScheduleAsync(Action<object> action, object state, TimeSpan delay, CancellationToken cancellationToken);

        Task ScheduleAsync(Action<object> action, object state, TimeSpan delay);

        Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken cancellationToken);

        Task ScheduleAsync(Action action, TimeSpan delay);

        Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay);

        Task ScheduleAsync(Action<object, object> action, object context, object state, TimeSpan delay, CancellationToken cancellationToken);


        Task<T1> SubmitAsync<T1>(Func<T1> func);

        Task<T1> SubmitAsync<T1>(Func<T1> func, CancellationToken cancellationToken);

        Task<T1> SubmitAsync<T1>(Func<object, T1> func, object state);

        Task<T1> SubmitAsync<T1>(Func<object, T1> func, object state, CancellationToken cancellationToken);

        Task<T1> SubmitAsync<T1>(Func<object, object, T1> func, object context, object state);

        Task<T1> SubmitAsync<T1>(Func<object, object, T1> func, object context, object state, CancellationToken cancellationToken);

        Task<T> ShutdownGracefullyAsync();

        Task<T> ShutdownGracefullyAsync(TimeSpan quietPeriod, TimeSpan timeout);
    }
}
