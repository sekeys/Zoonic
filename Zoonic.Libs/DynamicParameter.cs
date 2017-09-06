using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Zoonic.Lib
{
    public class DynamicParameter : DynamicObject//,IEnumerable<object>,IEnumerator<object>
    {
        public static bool UseStrict { get; set; } = false;
        private ConcurrentDictionary<string, object> Dict = new ConcurrentDictionary<string, object>();
        public object this[string field]
        {
            get
            {
                if (!UseStrict) { field = field.ToLower(); }
                object obj = null;
                Dict.TryGetValue(field, out obj); return obj;
            }
            set => Dict.TryAdd(UseStrict ? field : field.ToLower(), value);
        }

        public int Count
        {
            get => Dict.Count;
        }

        public bool Contains(string field)
        {
            return Dict.ContainsKey(UseStrict ? field : field.ToLower());
        }

        public object Get(string field)
        {
            field = UseStrict ? field : field.ToLower();
            return Dict.ContainsKey(field) ? Dict[field] : null;
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

        public bool Is<T>(string field) => Get(field) is T;

        public void Set(string field, object value) => this[field] = value;
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;
            return true;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return true;
        }
        public string String(string key)
        {
            Require.NotNull(key);
            if (this.Contains(key))
            {
                return this[key].ToString();
            }
            return "";
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

        public DynamicParameter Merge(IDictionary<string, object> keyvaluePairs)
        {

            foreach (var keyvalue in keyvaluePairs)
            {
                this[keyvalue.Key] = keyvalue.Value;
            }
            return this;
        }

        public DynamicParameter Merge(ICollection<KeyValuePair<string, object>> keyvaluePairs)
        {

            foreach (var keyvalue in keyvaluePairs)
            {
                this[keyvalue.Key] = keyvalue.Value;
            }
            return this;
        }
        public DynamicParameter Merge(IEnumerable<KeyValuePair<string, object>> keyvaluePairs)
        {

            foreach (var keyvalue in keyvaluePairs)
            {
                this[keyvalue.Key] = keyvalue.Value;
            }
            return this;
        }
        public DynamicParameter Merge<T>(IDictionary<string, T> keyvaluePairs)
        {

            foreach (var keyvalue in keyvaluePairs)
            {
                this[keyvalue.Key] = keyvalue.Value;
            }
            return this;
        }

        public DynamicParameter Merge<T>(ICollection<KeyValuePair<string, T>> keyvaluePairs)
        {
            foreach (var keyvalue in keyvaluePairs)
            {
                this[keyvalue.Key] = keyvalue.Value;
            }
            return this;
        }

        public DynamicParameter Merge<T>(IEnumerable<KeyValuePair<string, T>> keyvaluePairs)
        {
            foreach (var keyvalue in keyvaluePairs)
            {
                this[keyvalue.Key] = keyvalue.Value;
            }
            return this;
        }
    }
}
