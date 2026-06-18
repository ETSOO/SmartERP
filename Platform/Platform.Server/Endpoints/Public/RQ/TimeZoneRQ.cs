using com.etsoo.CoreFramework.Models;

namespace Platform.Server.Endpoints.Public.RQ
{
    /// <summary>
    /// Time zone request data
    /// 时区请求数据
    /// </summary>
    public record TimeZoneRQ : QueryRQ
    {
        /// <summary>
        /// Culture, like "en-US", "zh-CN", etc. If not specified, the current culture will be used.
        /// 文化
        /// </summary>
        public string? Culture { get; init; }

        /// <summary>
        /// All system timezones
        /// 所有系统时区
        /// </summary>
        public bool? All { get; init; }
    }
}
