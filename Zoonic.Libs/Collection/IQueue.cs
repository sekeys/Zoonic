using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Collection
{
    public interface IQueue<T>
    {
        void Enqueue(T item);

        T Dequeue();

        T Peek();

        int Count { get; }

        bool IsEmpty { get; }

        void Clear();

        IQueue<T> Clone();
    }
}
