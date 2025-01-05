using Microsoft.EntityFrameworkCore;

namespace PlatformShared.Database
{
    /// <summary>
    /// Database custom functions
    /// 数据库自定义函数
    /// </summary>
    public static class MyDbFunctions
    {
        /// <summary>
        /// Hide data
        /// 隐藏数据
        /// </summary>
        /// <param name="data">Source data</param>
        /// <param name="endChar">Optional end char</param>
        /// <returns>Result</returns>
        /// <exception cref="NotSupportedException"></exception>
        public static string HideData(string? data, char? endChar = null)
            => throw new NotSupportedException();

        /// <summary>
        /// Register custom functions
        /// 注册自定义函数
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        public static void Register(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFunction(typeof(MyDbFunctions).GetMethod(nameof(HideData), [typeof(string), typeof(char?)])!)
                .HasName("hide_data");
        }
    }
}
