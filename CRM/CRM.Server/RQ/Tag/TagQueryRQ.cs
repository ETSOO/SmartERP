using PlatformShared.Database.Models;

namespace CRM.Server.RQ.Tag
{
    /// <summary>
    /// Feature tag query request data
    /// 特征标签查询请求数据
    /// </summary>
    public record TagQueryRQ : QueryIntRQ
    {
        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FeatureTagKind? Kind { get; init; }
    }
}
