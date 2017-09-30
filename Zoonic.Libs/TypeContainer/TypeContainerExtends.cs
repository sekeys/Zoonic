

namespace Zoonic
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public static class TypeContainerExtends
    {
        public static T Fetch<T>(this ITypeContainer container)
        {
            return Constructor.Construct<T>();
        }
        public static object Fetch(this ITypeContainer container,Type baseType)
        {
            return Constructor.Construct(baseType);
        }
        
    }
}
