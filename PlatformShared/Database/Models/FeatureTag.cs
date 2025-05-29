namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Feature tag kind, compatible with IdentityTypeFlags
    /// 特征标签类型，和IdentityTypeFlags兼容
    /// </summary>
    public enum FeatureTagKind : short
    {
        User = 1,
        Customer = 2,
        Supplier = 4,
        Contact = 8,
        Org = 16,
        Dept = 32,
        Product = 256,
        Order = 512
    }

    /// <summary>
    /// Feature tag
    /// 特征标签
    /// </summary>
    public class FeatureTag
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Core organization Id
        /// 核心机构编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public FeatureTagKind Kind { get; set; }

        /// <summary>
        /// Tag
        /// 标签
        /// </summary>
        public string Tag { get; set; } = default!;

        /// <summary>
        /// Reference count
        /// 引用次数
        /// </summary>
        public int Total { get; set; }
    }
}
