using PlatformShared.Database.Models;

namespace CRM.Server.Dto.Tag
{
    /// <summary>
    /// Tag query data
    /// 标签查询数据
    /// </summary>
    public record TagQueryData
    {
        /// <summary>
        /// Tag id
        /// 标签ID
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FeatureTagKind Kind { get; init; }

        /// <summary>
        /// Tag
        /// 标签
        /// </summary>
        public required string Tag { get; init; }

        /// <summary>
        /// Total
        /// 数量
        /// </summary>
        public int Total { get; init; }
    }
}
