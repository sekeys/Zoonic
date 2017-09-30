using System;
using System.Collections.Generic;
using System.Text;
using Zoonic;

namespace Zoonic.Configuration
{
    public class Appsetting:DynamicParameter
    {
        private static Appsetting _Setting;
        private static object Locker = new object();
        public static Appsetting AppSettings
        {
            get
            {
                if (_Setting == null)
                {
                    lock (Locker)
                    {
                        if (_Setting == null) _Setting = new Appsetting();
                    }
                }
                return _Setting;
            }
        }
    }
}
