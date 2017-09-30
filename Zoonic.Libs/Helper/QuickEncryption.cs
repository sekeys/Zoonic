using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Zoonic
{
    public class QuickEncryption
    {
        #region 使用对称加密、解密

        /// <summary>
        /// 使用对称算法加密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encryptKey"></param>
        /// <returns></returns>
        public static string SymmetricEncrypts(string str, string encryptKey)
        {
            string result = string.Empty;
            byte[] inputData = System.Text.Encoding.UTF8.GetBytes(str);
            byte[] IV = { 0x77, 0x70, 0x50, 0xD9, 0xE1, 0x7F, 0x23, 0x13, 0x7A, 0xB3, 0xC7, 0xA7, 0x48, 0x2A, 0x4B, 0x39 };
            try
            {
                byte[] byKey = System.Text.Encoding.UTF8.GetBytes(encryptKey);
                //如需指定加密算法，可在Create()参数中指定字符串
                //Create()方法中的参数可以是：DES、RC2 System、Rijndael、TripleDES 
                //采用不同的实现类对IV向量的要求不一样(可以用GenerateIV()方法生成)，无参数表示用Rijndael
                SymmetricAlgorithm Algorithm = Aes.Create();//产生一种加密算法
                MemoryStream msTarget = new MemoryStream();
                //定义将数据流链接到加密转换的流。
                CryptoStream encStream = new CryptoStream(msTarget, Algorithm.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
                encStream.Write(inputData, 0, inputData.Length);
                encStream.FlushFinalBlock();
                result = Convert.ToBase64String(msTarget.ToArray());
            }
            catch (Exception ex)
            {
                return null;
            }
            return result;
        }


        /// <summary>
        /// 使用对称算法解密
        /// </summary>
        /// <param name="encryptStr"></param>
        /// <param name="encryptKey"></param>
        /// <returns></returns>
        public static string SymmectricDecrypts(string encryptStr, string encryptKey)
        {
            string result = string.Empty;
            //加密时使用的是Convert.ToBase64String(),解密时必须使用Convert.FromBase64String()
            try
            {
                byte[] encryptData = Convert.FromBase64String(encryptStr);
                byte[] byKey = System.Text.Encoding.UTF8.GetBytes(encryptKey);
                byte[] IV = { 0x77, 0x70, 0x50, 0xD9, 0xE1, 0x7F, 0x23, 0x13, 0x7A, 0xB3, 0xC7, 0xA7, 0x48, 0x2A, 0x4B, 0x39 };
                SymmetricAlgorithm Algorithm = Aes.Create();
                MemoryStream msTarget = new MemoryStream();
                CryptoStream decStream = new CryptoStream(msTarget, Algorithm.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
                decStream.Write(encryptData, 0, encryptData.Length);
                decStream.FlushFinalBlock();
                result = System.Text.Encoding.UTF8.GetString(msTarget.ToArray());
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }

        #endregion



        /// <summary>
        /// MD5加密
        /// </summary>
        public static string Md532(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("未将对象引用设置到对象的实例。");
            }

            var encoding = Encoding.UTF8;
            MD5 md5 = MD5.Create();
            return HashAlgorithmBase(md5, value, encoding);
        }

        /// <summary>
        /// 加权MD5加密
        /// </summary>
        public static string Md532(string value, string salt)
        {
            return salt == null ? Md532(value) : Md532(value + "[" + salt + "]");
        }

        /// <summary>
        /// HmacSha1 加密
        /// </summary>
        public static string HmacSha1( string value, string keyVal)
        {
            if (value == null)
            {
                throw new ArgumentNullException("未将对象引用设置到对象的实例。");
            }
            var encoding = Encoding.UTF8;
            byte[] keyStr = encoding.GetBytes(keyVal);
            HMACSHA1 hmacSha1 = new HMACSHA1(keyStr);
            return HashAlgorithmBase(hmacSha1, value, encoding);
        }

        /// <summary>
        /// HmacSha256 加密
        /// </summary>
        public static string HmacSha256(string value, string keyVal)
        {
            if (value == null)
            {
                throw new ArgumentNullException("未将对象引用设置到对象的实例。");
            }
            var encoding = Encoding.UTF8;
            byte[] keyStr = encoding.GetBytes(keyVal);
            HMACSHA256 hmacSha256 = new HMACSHA256(keyStr);
            return HashAlgorithmBase(hmacSha256, value, encoding);
        }

        /// <summary>
        /// HmacSha384 加密
        /// </summary>
        public static string HmacSha384(string value, string keyVal)
        {
            if (value == null)
            {
                throw new ArgumentNullException("未将对象引用设置到对象的实例。");
            }
            var encoding = Encoding.UTF8;
            byte[] keyStr = encoding.GetBytes(keyVal);
            HMACSHA384 hmacSha384 = new HMACSHA384(keyStr);
            return HashAlgorithmBase(hmacSha384, value, encoding);
        }

        /// <summary>
        /// HmacSha512 加密
        /// </summary>
        public static string HmacSha512(string value, string keyVal)
        {
            if (value == null)
            {
                throw new ArgumentNullException("未将对象引用设置到对象的实例。");
            }
            var encoding = Encoding.UTF8;
            byte[] keyStr = encoding.GetBytes(keyVal);
            HMACSHA512 hmacSha512 = new HMACSHA512(keyStr);
            return HashAlgorithmBase(hmacSha512, value, encoding);
        }

        /// <summary>
        /// HmacMd5 加密
        /// </summary>
        public static string HmacMd5(string value, string keyVal)
        {
            if (value == null)
            {
                throw new ArgumentNullException("未将对象引用设置到对象的实例。");
            }
            var encoding = Encoding.UTF8;
            byte[] keyStr = encoding.GetBytes(keyVal);
            HMACMD5 hmacMd5 = new HMACMD5(keyStr);
            return HashAlgorithmBase(hmacMd5, value, encoding);
        }

        private static string HashAlgorithmBase(HashAlgorithm hashAlgorithmObj, string source, Encoding encoding)
        {
            byte[] btStr = encoding.GetBytes(source);
            byte[] hashStr = hashAlgorithmObj.ComputeHash(btStr);
            return Bytes2Str(hashStr);
        }
        private static string Bytes2Str(IEnumerable<byte> source, string formatStr = "{0:X2}")
        {
            StringBuilder pwd = new StringBuilder();
            foreach (byte btStr in source) { pwd.AppendFormat(formatStr, btStr); }
            return pwd.ToString();
        }
    }

}
