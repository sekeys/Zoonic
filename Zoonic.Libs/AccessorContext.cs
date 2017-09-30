using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic
{
    public sealed class AccessorContext:IDisposable
    {
        //static readonly AccessorStorage storage;
        //protected AccessorContext()
        //{
        //    Accessor<AccessorContext>.Current = new AccessorContext();
        //}
        const string DEFAULT = "ACCESSORCONTEXT.DEFAULT";
        readonly string name;
        public string Name => name;
        internal AccessorStorage localStorage;
        public AccessorContext() : this(DEFAULT)
        {
        }
        public AccessorContext(string name)
        {
            this.name = name;
            localStorage = new AccessorStorage(this);
            //localStorage.AddOrUpdate(name);
            Storage.AddOrUpdate(name, localStorage);
        }
        public T Get<T>(string name)
        {
            return localStorage.Get<T>(name);
        }
        public object Get(string name)
        {
            return localStorage.Get(name);
        }
        public T Get<T>()
        {
            return localStorage.Get<T>();
        }

        public void Set(string name,object value)
        {
            lock (this)
            {
                localStorage.AddOrUpdate(name, value);
            }
        }
        public void Set<T>(string name, T value)
        {
            lock (this)
            {
                localStorage.AddOrUpdate(name, value);
            }
        }
        public void Set<T>(T value) => this.Set(typeof(T).Name, value);
        public void Remove(string name)
        {
            lock (this)
            {
                localStorage.Remove(name);
            }
        }
        public void Remove(object value)
        {
            lock (this)
            {
                localStorage.Remove(value);
            }
        }
        public void Remove<T>()
        {
            lock (this)
            {
                localStorage.Remove<T>();
            }
        }
        static object locker = new object();
        static AccessorStorage Storage
        {
            get
            {
                var storage = Accessor<AccessorStorage>.Current;
                if (storage == null)
                {
                    lock (locker)
                    {
                        if (storage == null)
                        {
                            storage = new AccessorStorage(null);
                            Accessor<AccessorStorage>.Current = storage;
                        }
                    }
                }
                return storage;
            }
        }
        
        static AccessorStorage Default
        {
            get
            {
                var item= Storage.Get<AccessorStorage>(DEFAULT);
                if (item == null)
                {
                    new AccessorContext();
                }
                return Storage.Get<AccessorStorage>(DEFAULT);
            }
        }
        public static AccessorContext DefaultContext
        {
            get
            {
                return Default?.Context;
            }
        }
        public static T Gets<T>(string name)
        {
            return Storage.Get<T>(name);
        }
        public static object Gets(string name)
        {
            return Storage.Get(name);
        }
        public static T Gets<T>()
        {
            return Storage.Get<T>();
        }

        public static void Sets(string name, object value)
        {
            Storage.AddOrUpdate(name, value);
        }
        public static void Sets<T>(string name, T value)
        {
            Storage.AddOrUpdate(name, value);
        }
        public static void Sets<T>(T value) => Sets(nameof(T), value);
        public static void Removes(string name)
        {
            Storage.Remove(name);
        }
        public static void Removes(object value)
        {
            Storage.Remove(value);
        }
        public void Remove()
        {
            Storage.Remove(name);
        }
        public static void Removes<T>()
        {
            Storage.Remove<T>();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    GC.Collect();
                }
                
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
         ~AccessorContext() {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
    sealed class AccessorStorage
    {
        readonly AccessorStorageItem head;
        AccessorStorageItem tail;
        public AccessorContext Context { get;private set; }
        public AccessorStorage(AccessorContext context)
        {
            head = new HeadAccessorStorageItem();
            tail = null;
            head.Next = tail;
            Context = context;
        }

        public void AddOrUpdate(string name, object value)
        {
            var item = Item(name);
            if (item == null)
            {
                Add(new AccessorStorageItem() { Name = name, Value = value });
            }
            else
            {

                item.Value = value;
            }
        }
        public void AddOrUpdate(object value)
        {
            AddOrUpdate(nameof(value), value);
        }
        public void Add(AccessorStorageItem accessorStorageItem)
        {
            if (tail == null)
            {
                tail = accessorStorageItem;
                if (this.head.Next == null)
                {
                    this.head.Next = tail;
                }
            }
            else
            {
                tail.Next = accessorStorageItem;
                tail = accessorStorageItem;
            }
        }

        public AccessorStorageItem Item(string name)
        {
            AccessorStorageItem context = this.head.Next;
            while (context != null)
            {
                if (context.Name.Equals(name, StringComparison.Ordinal))
                {
                    return context;
                }
                context = context.Next;
            }
            return null;
        }
        public AccessorStorageItem Item<T>()
        {
            AccessorStorageItem ctx = this.head.Next;
            while (true)
            {
                if (ctx == null)
                {
                    return null;
                }
                if (ctx.Value is T)
                {
                    return ctx;
                }
                ctx = ctx.Next;
            }
        }
        public object Get(string name)
        {
            var item = Item(name);
            return item?.Value;
        }
        public T Get<T>()
        {

            var item = Item<T>();
            return item == null ? default(T) : (T)item.Value;
        }
        public T Get<T>(string name)
        {
            var item = Item(name);
            return item == null ? default(T) : (T)item.Value;
        }
        public void Remove(string name)
        {
            AccessorStorageItem ctx = this.head.Next;
            var pre = this.head;
            while (ctx != null)
            {
                if (ctx.Name.Equals(name, StringComparison.Ordinal))
                {
                    pre.Next = ctx.Next;
                }
                pre = ctx;
                ctx = ctx.Next;
            }
        }
        public void Remove<T>()
        {
            AccessorStorageItem ctx = this.head.Next;
            var pre = this.head;
            while (true)
            {
                if (ctx == null)
                {
                    return;
                }
                if (ctx.Value is T)
                {
                    pre.Next = ctx.Next;
                }
                pre = ctx;
                ctx = ctx.Next;
            }
        }
        public void Remove(object value)
        {
            if (value == null) return;
            AccessorStorageItem ctx = this.head.Next;
            var pre = this.head;
            while (true)
            {
                if (ctx == null)
                {
                    return;
                }
                if (ctx.Value !=null &&ctx.Value.Equals(value))
                {
                    pre.Next = ctx.Next;
                }
                pre = ctx;
                ctx = ctx.Next;
            }
        }
    }
    class AccessorStorageItem
    {
        public AccessorStorageItem Next;
        public string Name { get; set; }
        public object Value { get; set; }

    }
    sealed class HeadAccessorStorageItem : AccessorStorageItem
    {

    }
}
