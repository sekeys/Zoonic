

namespace Zoonic.Concurrency
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Zoonic;

    public interface IScheduledTask
    {
        bool Cancel();
        PreciseTimeSpan Deadline { get; }
        Task Completion { get; }
        TaskAwaiter GetAwaiter();
    }
}
