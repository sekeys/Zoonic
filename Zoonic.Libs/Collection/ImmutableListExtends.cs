using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib.Collection
{
    public static class ImmutableListExtends

    {
        public static ImmutableList<T> AsImmutableList<T>(this IEnumerable<T> enumerable)
        => new ImmutableList<T>(enumerable);
    }
}
