

namespace Zoonic.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    public static class StringExtensions
    {
        public static int ToInt32(this string str)
        {
            return Convert.ToInt32(str);
        }
        public static short ToInt16(this string str)
        {
            return Convert.ToInt16(str);
        }
        public static long ToInt64(this string str)
        {
            return Convert.ToInt64(str);
        }

        public static DateTime ToDateTime(this string str, DateTime value)
        {
            try
            {
                return Convert.ToDateTime(str);
            }
            catch
            {
                return value;
            }
        }
        public static decimal ToDecimal(this string str, decimal value)
        {
            try
            {
                return Convert.ToDecimal(str);
            }
            catch
            {
                return value;
            }
        }
        public static double ToDouble(this string str, double value)
        {
            try
            {
                return Convert.ToDouble(str);
            }
            catch
            {
                return value;
            }
        }
        public static int ToInt32(this string str, int value)
        {
            try
            {
                return Convert.ToInt32(str);
            }
            catch
            {
                return value;
            }
        }
        public static short ToInt16(this string str, short value)
        {
            try
            {
                return Convert.ToInt16(str);
            }
            catch
            {
                return value;
            }
        }
        public static long ToInt64(this string str, long value)
        {
            try
            {
                return Convert.ToInt64(str);
        }
            catch
            {
                return value;
            }
}

        public static bool ToBoolean(this string str)
        {
            return str.StartsWith("T", StringComparison.OrdinalIgnoreCase)||"1".Equals(str) ? true : false;
        }
        
        /// <summary>
        /// 获得当前绝对路径，同时兼容windows和linux（系统自带的都不兼容）。
        /// </summary>
        /// <param name="strPath">指定的路径，支持/|./|../分割</param>
        /// <returns>绝对路径，不带/后缀</returns>
        public static string CombinePath(this string rootPath,string strPath)
        {


            if (strPath == null)
            {
                return rootPath;
            }
            else
            {
                List<string> prePath = rootPath.Split(
                    new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> srcPath = strPath.Split(
                    new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                ComputePath(prePath, srcPath);
                Console.WriteLine(rootPath);
                if (prePath.Count > 0 && prePath[0].Contains(":"))//windows
                {
                    if (prePath.Count == 1)
                    {
                        return prePath[0] + "/";
                    }
                    else
                    {
                        return String.Join("/", prePath);
                    }
                }
                else//linux
                {
                    return "/" + String.Join("/", prePath);
                }
            }
        }

        ///<summary>
        ///支持相对路径的./和../
        ///</summary>
        private static void ComputePath(List<string> prePath, List<string> srcPath)
        {
            var precount = prePath.Count;
            foreach (string src in srcPath)
            {
                if (src == "..")
                {
                    if (precount > 1 || (precount == 1 && !prePath[0].Contains(":")))
                    {
                        prePath.RemoveAt(--precount);
                    }
                }
                else if (src != ".")
                {
                    prePath.Add(src);
                    precount++;
                }
            }
        }
    }
}
