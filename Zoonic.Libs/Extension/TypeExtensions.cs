using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Extensions
{
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.Loader;

    public static class TypeExtensions
    {
        public static string Description<T>(this Type type)
        {
            if (type == null) { return null; }
            var attr = type.GetTypeInfo().GetCustomAttribute<DescriptionAttribute>();
            if (attr == null) { return null;}
            return attr.Description;
        }
    }
}
