

namespace Zoonic
{
    using System.Threading;
    /// <summary>
    /// 解决线程数据存储问题，
    /// 1.父类创建，并保存数据结构则在子线程中也能共享此数据。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Accessor<T>
    {
        static AsyncLocal<T> _asyncLocalContext = new AsyncLocal<T>();
        public static T Current
        {
            get => _asyncLocalContext.Value;
            set => _asyncLocalContext.Value = value;
        }
    }
}
