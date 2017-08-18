using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib.Collection
{
    using System.Collections.Concurrent;
    public class FriendlyConcurrentQueue<T>:ConcurrentQueue<T>,IConcurrentQueue<T>
    {
        public bool TryEnqueue(T element)
        {
            this.Enqueue(element);
            return true;
        }

        void IConcurrentQueue<T>.Clear()
        {
            T item;
            while (this.TryDequeue(out item))
            {
            }
        }
    }
}
