
namespace Zoonic.Collection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ImmutableList<T>
    {
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();

        private readonly T[] _data;

        public ImmutableList()
        {
            _data = new T[0];
        }

        public ImmutableList(T[] data)
        {
            _data = data;
        }

        public ImmutableList(IEnumerable<T> data)
        {
            _data = data.ToArray();
        }

        public T[] Data
        {
            get
            {
                return _data;
            }
        }

        public ImmutableList<T> Add(T value)
        {
            var newData = new T[_data.Length + 1];

            Array.Copy(_data, newData, _data.Length);
            newData[_data.Length] = value;

            return new ImmutableList<T>(newData);
        }

        public ImmutableList<T> Remove(T value)
        {
            var i = IndexOf(value);
            if (i < 0)
                return this;

            var length = _data.Length;
            if (length == 1)
                return Empty;

            var newData = new T[length - 1];

            Array.Copy(_data, 0, newData, 0, i);
            Array.Copy(_data, i + 1, newData, i, length - i - 1);

            return new ImmutableList<T>(newData);
        }

        public int IndexOf(T value)
        {
            for (var i = 0; i < _data.Length; ++i)
                if (object.Equals(_data[i], value))
                    return i;

            return -1;
        }
        public bool Contains(T value)
        {
            return IndexOf(value) > -1;
        }
    }
}
