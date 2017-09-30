using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Collection
{
    public class Queue<T> : System.Collections.Generic.Queue<T>, IQueue<T>
    {
        public Queue(IEnumerable<T> collection):base(collection)
        {

        }
        public Queue() { }
        public bool IsEmpty => base.Count==0;

        public IQueue<T> Clone()
        {
            T[] ar = new T[this.Count];
            this.ToArray().CopyTo(ar,0);
            return new Queue<T>(ar);
        }
    }
}
