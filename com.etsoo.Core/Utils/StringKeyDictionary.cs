using System.Collections.Generic;

namespace com.etsoo.Core.Utils
{
    /// <summary>
    /// String key extended dictionary
    /// 字符键扩展字典
    /// </summary>
    /// <typeparam name="V">Value type</typeparam>
    public class StringKeyDictionary<V> : Dictionary<string, V>
    {
        /// <summary>
        /// Get value
        /// 获取值
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public T? Get<T>(string key) where T : struct
        {
            V value;
            if (this.TryGetValue(key, out value))
            {
                if (value == null)
                {
                    return null;
                }
                else if (value is T t)
                {
                    return t;
                }
                else
                {
                    return ParseUtil.TryParse<T>(value.ToString());
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get value with default value
        /// 获取值，提供默认值
        /// </summary>
        /// <typeparam name="T">Struct type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Value</returns>
        public T Get<T>(string key, T defaultValue) where T : struct
        {
            return this.Get<T>(key).GetValueOrDefault(defaultValue);
        }

        /// <summary>
        /// Get string value
        /// 获取字符串值
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>String value</returns>
        public string Get(string key)
        {
            V value;
            if (this.TryGetValue(key, out value))
            {
                if (value == null)
                    return null;

                return value.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}