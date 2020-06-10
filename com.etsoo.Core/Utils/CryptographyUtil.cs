using System;
using System.Security.Cryptography;
using System.Text;

namespace com.etsoo.Core.Utils
{
    /// <summary>
    /// Cryptography Tools
    /// 密码工具
    /// </summary>
    public static class CryptographyUtil
    {
        /// <summary>
        /// Hash-based Message Authentication Code (HMAC)
        /// </summary>
        /// <param name="message">Raw message</param>
        /// <param name="privateKey">Private key</param>
        /// <returns>Hashed message</returns>
        /// <seealso href="http://www.baike.com/wiki/HMAC/">HMAC</seealso>
        public static string HMACSHA512(string message, string privateKey)
        {
            return Convert.ToBase64String(HMACSHA512Bytes(message, privateKey), Base64FormattingOptions.None);
        }

        /// <summary>
        /// Hash-based Message Authentication Code (HMAC)
        /// </summary>
        /// <param name="message">Raw message</param>
        /// <param name="privateKey">Private key</param>
        /// <returns>Hashed bytes</returns>
        /// <seealso href="http://www.baike.com/wiki/HMAC/">HMAC</seealso>
        public static byte[] HMACSHA512Bytes(string message, string privateKey)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            using (var alg = new HMACSHA512(Encoding.UTF8.GetBytes(privateKey)))
            {
                alg.Initialize();
                alg.ComputeHash(Encoding.UTF8.GetBytes(message));
                return alg.Hash;
            }
        }
    }
}
