namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Custom culture resources
    /// 个性化文化资源
    /// </summary>
    public class FeatureCulture
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Key
        /// 键名
        /// </summary>
        public string Key { get; set; } = default!;

        /// <summary>
        /// Culture, for example zh-CN
        /// 文化，比如 zh-CN
        /// </summary>
        public string Culture { get; set; } = default!;

        /// <summary>
        /// Organization Id, null means global
        /// 所属机构，null 表示全局
        /// </summary>
        public int? CoreOrganizationId { get; set; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Json data
        /// JSON 数据
        /// </summary>
        public string? JsonData { get; set; }

        /// <summary>
        /// Core organization
        /// 所属机构
        /// </summary>
        public CoreOrganization? CoreOrganization { get; set; }
    }
}
