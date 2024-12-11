using Microsoft.EntityFrameworkCore;

namespace PlatformShared.Database
{
    /// <summary>
    /// Database utilities
    /// 数据库工具
    /// </summary>
    public static class DbUtils
    {
        /// <summary>
        /// ILike method data for QueryEtsooKeywords
        /// 用于 QueryEtsooKeywords 的ILike方法数据
        /// </summary>
        public static (Type type, string name) ILikeMethod => (typeof(NpgsqlDbFunctionsExtensions), nameof(NpgsqlDbFunctionsExtensions.ILike));
    }
}
