using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zoonic
{
    public static class Require
    {
        public static void NotNull(object obj)
        {
            if (obj == null)
            {
                throw new System.ArgumentNullException(nameof(obj));
            }
        }
        public static void NotNull(object obj, string paramName)
        {
            if (obj == null) { throw new System.ArgumentNullException(paramName); }
        }
        public static void NotNull(string obj)
        {
            if (string.IsNullOrEmpty(obj)) { throw new System.ArgumentNullException(nameof(obj)); }
        }
        public static void NotWhitespace(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) { throw new System.ArgumentNullException(); }

        }
        public static void Length(string str, int length)
        {
            if (str?.Length > length)
            {
                throw new System.ArgumentException($"字符串长度不能超过{str}");
            }
        }
    }
}
