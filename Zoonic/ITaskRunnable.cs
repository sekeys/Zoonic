

namespace Zoonic.Interface
{
    using System.Threading.Tasks;
    using Zoonic.Lib;

    public interface ITaskRunnable:IRunnable
    {
        Task Task { get; }
    }
    public interface ITaskRunnable<T> : IRunnable<T>
    {
        Task Task { get; }
    }
    public interface ITaskRunnable<T,T2> : IRunnable<T,T2>
    {
        Task Task { get; }
    }
}
