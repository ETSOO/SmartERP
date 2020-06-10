using System;
using System.Collections.Generic;

namespace com.etsoo.Core.Utils
{
    /// <summary>
    /// Parse functions
    /// </summary>
    public static class ParseUtil
    {
        /// <summary>
        /// Try to parse input object to target type value
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="input">Input content</param>
        /// <returns>Parsed value</returns>
        public static Nullable<T> TryParse<T>(object input) where T : struct
        {
            // Directly detect the null value and database side null value
            if (input == null || Convert.IsDBNull(input))
                return null;

            if (input is T v)
                return v;

            return TryParse<T>(input.ToString());
        }

        // TryParse generic delegate
        private delegate bool TryParseDelegate<T>(string s, out T input);

        /// <summary>
        /// Try to parse input content to target type value
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="input">Input content</param>
        /// <returns>Parsed value</returns>
        public static Nullable<T> TryParse<T>(string input) where T : struct
        {
            // Directly detect the null and empty value
            if (string.IsNullOrEmpty(input))
                return null;

            // Default value of the type
            var defaultValue = default(T);

            // Custom format and parser
            TryParseDelegate<T> parser;
            if (defaultValue is bool)
            {
                if (input == "1" || input == "on")
                    input = "true";
                else if (input == "0" || input == "off")
                    input = "false";

                parser = new TryParseDelegate<bool>(bool.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is int)
            {
                parser = new TryParseDelegate<int>(int.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is long)
            {
                parser = new TryParseDelegate<long>(long.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is DateTime)
            {
                parser = new TryParseDelegate<DateTime>(DateTime.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is Enum)
            {
                parser = new TryParseDelegate<T>(Enum.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is double)
            {
                parser = new TryParseDelegate<double>(double.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is float)
            {
                parser = new TryParseDelegate<float>(float.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is decimal)
            {
                parser = new TryParseDelegate<decimal>(decimal.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is byte)
            {
                parser = new TryParseDelegate<byte>(byte.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is Guid)
            {
                parser = new TryParseDelegate<Guid>(Guid.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is char)
            {
                parser = new TryParseDelegate<char>(char.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is short)
            {
                parser = new TryParseDelegate<short>(short.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is ushort)
            {
                parser = new TryParseDelegate<ushort>(ushort.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is uint)
            {
                parser = new TryParseDelegate<uint>(uint.TryParse) as TryParseDelegate<T>;
            }
            else if (defaultValue is ulong)
            {
                parser = new TryParseDelegate<ulong>(ulong.TryParse) as TryParseDelegate<T>;
            }
            else
            {
                return null;
            }

            // Parse
            T newValue;
            if (parser(input, out newValue))
            {
                return newValue;
            }
            else
            {
                return null;
            }
        }
    }
}