using System;
using System.Collections.Generic;
using System.Text;

namespace Zoonic.Lib.Log
{
    public static class ObjectExtensions
    {
        public static void ThrowIfnull(this object obj)
        {
            if (obj == null)
            {
                throw new Exception($"{nameof(obj)} can't allow be null value");
            }
        }
        public static void ThrowIfnull(this object obj,string msg)
        {
            if (obj == null)
            {
                throw new Exception(string.Format(msg,nameof(msg)));
            }
        }

        public static void ThrowIfWhitespace(this object obj)
        {
            obj.ThrowIfnull();
            if (obj is string)
            {
                if (string.IsNullOrWhiteSpace(obj as string)) throw new Exception($"{nameof(obj)} can't allow be empty");
            }
        }
        public static void ThrowIfWhitespace(this object obj, string msg)
        {
            obj.ThrowIfnull( msg);
            if (obj is string)
            {
               if(string.IsNullOrWhiteSpace(obj as string)) throw new Exception(string.Format(msg, nameof(msg)));
            }
        }
        public static void Throw<T>(this object obj, Func<T, bool> func)
        {
            if (!(obj is T))
            {
                throw new Exception("the object's type is not special type");
            }
            if (!func((T)obj))
            {
                throw new Exception("not match the special rule");
            }
        }
        public static void Throw<T>(this object obj, Func<T, bool> func, string msg)
        {
            if(!(obj is T))
            {
                throw new Exception(msg);
            }
            if (!func((T)obj))
            {
                throw new Exception(msg);
            }
        }
        public static void Throw(this object obj,Func<object,bool> func, string msg)
        {
            if (!func(obj))
            {
                throw new Exception(msg);
            }
        }
        public static void Throw(this object obj, Func<object, bool> func)
        {
            if (!func(obj))
            {
                throw new Exception("not match the special rule");
            }
        }

        public static void DebugPackage()
        {

        }
        public static void AssertPackage()
        {

        }
        public static void Debug()
        {

        }

        public static void DebugOnShared()
        {

        }
        public static void AssertOnShared()
        {

        }

        public static void Assert()
        {
            
        }
    }
}
