using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zoonic
{
    public class Base64Encoder
    {
        public static string EncodeBase64(System.Text.Encoding encoding,string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }
            return Convert.ToBase64String(encoding.GetBytes(str));
        }

        public static string EncodeBase64(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
        }
        public static string DecodeBase64(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }
            byte[] output = Convert.FromBase64String(str);
            return System.Text.Encoding.UTF8.GetString(output);
        }
        public static string DecodeBase64(System.Text.Encoding encoding, string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }
            byte[] output = Convert.FromBase64String(str);
            return encoding.GetString(output);
        }
    }
}
