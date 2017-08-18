using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib.Collection
{
    public class ConcurrentPriorityQueue<T> : IConcurrentQueue<T>
    {
        public int Count => throw new NotImplementedException();

        public bool IsEmpty => throw new NotImplementedException();

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool TryDequeue(out T item)
        {
            throw new NotImplementedException();
        }

        public bool TryEnqueue(T item)
        {
            throw new NotImplementedException();
        }

        public bool TryPeek(out T item)
        {
            throw new NotImplementedException();
        }
    }
}
