using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Collection
{
    public interface IMapping<T1,T2>
    {
        T2 Map(T1 value);
    }
}
