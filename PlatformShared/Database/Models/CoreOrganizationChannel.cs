namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Core organization channel relationship
    /// 核心机构渠道关系
    /// </summary>
    public class CoreOrganizationChannel
    {
        /// <summary>
        /// Owner organization id
        /// 所有者机构编号
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// Partner organization id
        /// 伙伴机构编号
        /// </summary>
        public int PartnerId { get; set; }

        /// <summary>
        /// Enabled or not
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Creation
        /// 登记时间
        /// </summary>
        public DateTimeOffset Creation { get; set; }

        /// <summary>
        /// Refresh time
        /// 刷新时间
        /// </summary>
        public DateTimeOffset RefreshTime { get; set; }

        /// <summary>
        /// Owner organization
        /// 所有者机构
        /// </summary>
        public CoreOrganization Owner { get; set; } = default!;

        /// <summary>
        /// Partner organization
        /// 伙伴机构
        /// </summary>
        public CoreOrganization Partner { get; set; } = default!;
    }
}
