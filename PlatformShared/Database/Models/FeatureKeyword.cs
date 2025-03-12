namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Feature keyword kind, compatible with IdentityType
    /// 特征关键词类型，和IdentityType兼容
    /// </summary>
    public enum FeatureKeywordKind : short
    {
        User = 1,
        Customer = 2,
        Supplier = 4,
        Product = 256
    }

    /// <summary>
    /// Feature keyword
    /// 特征关键词
    /// </summary>
    public class FeatureKeyword
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tag
        /// 标签
        /// </summary>
        public string Tag { get; set; } = default!;

        /// <summary>
        /// Core organization Id
        /// 核心机构编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// Reference count
        /// 引用次数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FeatureKeywordKind Kind { get; set; }
    }
}
