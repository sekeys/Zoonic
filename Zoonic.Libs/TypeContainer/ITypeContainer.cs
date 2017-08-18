using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib
{
    public interface ITypeContainer
    {
        void Inject<baseType, ImpType>();
        void Inject(Type interType, Type impType);
        void Inject<T>(Type impType);
        Type Fetch(Type type);
        IEnumerable<Type> FetchInterface();
        IEnumerable<KeyValuePair<Type, Type>> Fetch();
        Type Fetch<T>();
    }
}
