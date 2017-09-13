

namespace Zoonic.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Dynamic;
    using System.Text;
    public class IgnoreDynamic : DynamicObject
    {
        private ConcurrentDictionary<string, object> concurrentDictionary;
        public IgnoreDynamic()
        {
            concurrentDictionary = new ConcurrentDictionary<string, object>();
        }
        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string key = binder.Name.ToLower();
            if (concurrentDictionary.ContainsKey(key))
            {
                concurrentDictionary.TryGetValue(key, out result);
            }
            else
            {
                result = null;
            }
            return true;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string key = binder.Name.ToLower();
            if (concurrentDictionary.ContainsKey(key))
            {
                concurrentDictionary.TryRemove(key, out value);
            }
            concurrentDictionary.TryAdd(key, value);
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            return base.TrySetIndex(binder, indexes, value);
        }

        public object this[string key]
        {
            get
            {
                object result = null;
                key = key.ToLower();
                if (concurrentDictionary.ContainsKey(key))
                {
                    concurrentDictionary.TryGetValue(key, out result);
                }
                return result;
            }
            set
            {
                key = key.ToLower();
                if (concurrentDictionary.ContainsKey(key))
                {
                    concurrentDictionary.TryRemove(key, out value);
                }
                concurrentDictionary.TryAdd(key, value);
            }
        }
        public bool Contains(string field)
        {
            return concurrentDictionary.ContainsKey(field.ToLower());
        }
        public object Get(string field)
        {
            field =  field.ToLower();
            return concurrentDictionary.ContainsKey(field) ? concurrentDictionary[field] : null;
        }

        public T Get<T>(string field) => (T)Get(field);


        public T Get<T>(string field, T defValue)
        {
            try
            {
                object obj = Get(field);
                return (T)obj;
            }
            catch (Exception ex)
            {
                return defValue;
            }
        }

        public T TypeOf<T>(string key)
        {
            Require.NotNull(key);
            if (this.Contains(key))
            {
                return (T)this[key];
            }
            return default(T);
        }
        public bool Is<T>(string field) => Get(field) is T;

        public void Set(string field, object value) => this[field] = value;
    }
}
