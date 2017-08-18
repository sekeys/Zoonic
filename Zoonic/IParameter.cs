

namespace Zoonic.Interface
{
    public interface IParameter
    {
        void Set(string field, object value);
        object Get(string field);
        T Get<T>(string field);
        T Get<T>(string field,T defValue);
        bool Is<T>(string field);
        object this[string field]
        {
            get;
            set;
        }

        bool Contains(string field);

        int Count { get;  }
    }
}
