using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zoonic.Collection
{
    public class ImmutableDictionary<T,T1>
    {
        public static readonly ImmutableDictionary<T,T1> Empty = new ImmutableDictionary<T, T1>();

        private readonly KeyValuePair<T,T1>[] _data;

        public ImmutableDictionary()
        {
            _data = new KeyValuePair<T, T1>[0];
        }

        protected ImmutableDictionary(KeyValuePair<T, T1>[] data)
        {
            _data = data;
        }

        public ImmutableDictionary(IEnumerable<KeyValuePair<T, T1>> data)
        {
            _data = data.ToArray();
        }
        public T1 this[T key]
        {
            get
            {
                var index = IndexOf(key);
                if (index>-1)
                {
                    return _data[index].Value;
                }
                return default(T1);
            }
        }
        public KeyValuePair<T, T1>[] Data
        {
            get
            {
                return _data;
            }
        }
        public virtual ImmutableDictionary<T, T1> Add(T key,T1 value)
        {
            var newData = new KeyValuePair<T, T1>[_data.Length + 1];

            Array.Copy(_data, newData, _data.Length);
            newData[_data.Length] = new KeyValuePair<T, T1>(key, value); // { Key = key, Value = value };

            return new ImmutableDictionary<T,T1>(newData);
        }

        public virtual ImmutableDictionary<T, T1> Add(KeyValuePair<T, T1> pair) => Add(pair.Key, pair.Value);
        public virtual ImmutableDictionary<T, T1> Remove(T value)
        {
            var i = IndexOf(value);
            if (i < 0)
                return this;

            var length = _data.Length;
            if (length == 1)
                return Empty;

            var newData = new KeyValuePair<T, T1>[length - 1];

            Array.Copy(_data, 0, newData, 0, i);
            Array.Copy(_data, i + 1, newData, i, length - i - 1);

            return new ImmutableDictionary<T, T1>(newData);
        }

        private int IndexOf(T key)
        {
            for (var i = 0; i < _data.Length; ++i)
                if (object.Equals(_data[i].Key, key))
                    return i;
            return -1;
        }
        private int IndexOf(KeyValuePair<T,T1> pair)
        {
            for (var i = 0; i < _data.Length; ++i)
                if (object.Equals(_data[i], pair))
                    return i;
            return -1;
        }
    }
}
