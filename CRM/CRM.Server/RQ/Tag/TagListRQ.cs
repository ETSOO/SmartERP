using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Tag
{
    /// <summary>
    /// Tag list request data
    /// 标签列表请求数据
    /// </summary>
    public record TagListRQ : QueryIntRQ
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FeatureTagKind Kind { get; init; }
    }
}
