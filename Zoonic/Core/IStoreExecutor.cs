using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Concurrency
{
    public interface IStorageExecutor:IExecutor
    {
        void Set(object value);
        void Set<T>(T value);
        T Get<T>();
        object Get(string name);
        void Set(string name, object value);
        void Set<T>(string name, T value);
    }
}
