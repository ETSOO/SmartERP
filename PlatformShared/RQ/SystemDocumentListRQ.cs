using com.etsoo.CoreFramework.Models;

namespace PlatformShared.RQ
{
    /// <summary>
    /// System Document list request data
    /// 系统文档列表请求数据
    /// </summary>
    public record SystemDocumentListRQ : QueryRQ<int>
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public string? Kind { get; init; }
    }
}
