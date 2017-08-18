using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib.Collection
{
    public class SortedList<T> : IList<T>, IEnumerable<T>,  IClonable<SortedList<T>>
    {
        readonly IComparer<T> comparer;
        int count;
        int capacity;
        T[] items;
        public SortedList(IComparer<T> comparer)
        {
            Require.NotNull(comparer);

            this.comparer = comparer;
            this.capacity = 11;
            this.items = new T[this.capacity];
        }

        public SortedList()
            : this(Comparer<T>.Default)
        {
        }
        public T this[int index]
        {
            get
            {
                if (index >= items.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return items[index];
            }
            set
            {
                if (index >= capacity)
                {
                    GrowHeap();
                }
                items[index]=value;
            }
        }
        public int Count => this.count;

        public bool IsEmpty => this.count == 0;

        public T Dequeue()
        {
            T result = this.Peek();
            if (result == null)
            {
                return default(T);
            }

            int newCount = --this.count;
            T lastItem = this.items[newCount];
            this.items[newCount] = null;
            if (newCount > 0)
            {
                this.TrickleDown(0, lastItem);
            }

            return result;
        }

        public void Enqueue(T item)
        {
            Require.NotNull(item);

            int oldCount = this.count;
            if (oldCount == this.capacity)
            {
                this.GrowHeap();
            }
            this.count = oldCount + 1;
            this.BubbleUp(oldCount, item);
        }

        public void Remove(T item)
        {
            int index = Array.IndexOf(this.items, item);
            if (index == -1)
            {
                return;
            }

            this.count--;
            if (index == this.count)
            {
                this.items[index] = default(T);
            }
            else
            {
                T last = this.items[this.count];
                this.items[this.count] = default(T);
                this.TrickleDown(index, last);
                if (this.items[index].Equals(last))
                {
                    this.BubbleUp(index, last);
                }
            }
        }

        void BubbleUp(int index, T item)
        {
            // index > 0 means there is a parent
            while (index > 0)
            {
                int parentIndex = (index - 1) >> 1;
                T parentItem = this.items[parentIndex];
                if (this.comparer.Compare(item, parentItem) >= 0)
                {
                    break;
                }
                this.items[index] = parentItem;
                index = parentIndex;
            }
            this.items[index] = item;
        }

        void GrowHeap()
        {
            int oldCapacity = this.capacity;
            this.capacity = oldCapacity + (oldCapacity <= 64 ? oldCapacity + 2 : (oldCapacity >> 1));
            var newHeap = new T[this.capacity];
            Array.Copy(this.items, 0, newHeap, 0, this.count);
            this.items = newHeap;
        }

        void TrickleDown(int index, T item)
        {
            int middleIndex = this.count >> 1;
            while (index < middleIndex)
            {
                int childIndex = (index << 1) + 1;
                T childItem = this.items[childIndex];
                int rightChildIndex = childIndex + 1;
                if (rightChildIndex < this.count
                    && this.comparer.Compare(childItem, this.items[rightChildIndex]) > 0)
                {
                    childIndex = rightChildIndex;
                    childItem = this.items[rightChildIndex];
                }
                if (this.comparer.Compare(item, childItem) <= 0)
                {
                    break;
                }
                this.items[index] = childItem;
                index = childIndex;
            }
            this.items[index] = item;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < this.count; i++)
            {
                yield return this.items[i];
            }
        }
        

        public void Clear()
        {
            this.count = 0;
            Array.Clear(this.items, 0, 0);
        }

        public bool IsReadOnly => false;


        public void Add(T item)
        {
            BubbleUp(0,item);
        }
        


        public bool Contains(T item)
        {
            foreach(var jtem in items)
            {
                if (item.Equals(jtem))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(items, array, arrayIndex);
        }
        bool Disposing = false;
        public void Dispose()
        {
            if (!Disposing)
            {
                GC.Collect();
            }
        }
        

        public int IndexOf(T item)
        {
            return Array.IndexOf(items, item);
        }

        public void Insert(int index, T item)
        {
            BubbleUp(index, item);
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }
        

        public void RemoveAt(int index)
        {
            Remove(items[index]);
        }
        

        SortedList<T> IClonable<SortedList<T>>.Clone()
        {
            throw new NotImplementedException();
        }
        
    }
}
