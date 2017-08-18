

namespace Zoonic.Concurrency
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Zoonic.Lib;

    public interface IScheduledTask<T>
    {
        bool Cancel();
        PreciseTimeSpan Deadline { get; }
        Task<T> Completion { get; }
        TaskAwaiter<T> GetAwaiter();
    }
}
