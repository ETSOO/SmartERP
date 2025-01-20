using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Core organization
    /// 核心机构
    /// </summary>
    public class CoreOrganization
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Owner id
        /// 所有人编号
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// Name
        /// 名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Brand
        /// 品牌
        /// </summary>
        public string? Brand { get; set; }

        /// <summary>
        /// Logo
        /// 标志
        /// </summary>
        public string? Logo { get; set; }

        /// <summary>
        /// PIN
        /// 唯一标识
        /// </summary>
        public string? Pin { get; set; }

        /// <summary>
        /// Parent organization id
        /// 父机构编号
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Global unique identifier, activated manually
        /// 全局唯一标识符，手动激活
        /// </summary>
        public Guid? Uid { get; set; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; } = EntityStatus.Normal;

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Query keyword
        /// 查询关键字，中文下默认使用拼音首字母
        /// </summary>
        public string? QueryKeyword { get; set; }

        /// <summary>
        /// Region
        /// 所在地区
        /// </summary>
        public required string Region { get; set; }

        /// <summary>
        /// Core logs
        /// 核心日志
        /// </summary>
        public ICollection<CoreLog> CoreLogs { get; set; } = [];

        /// <summary>
        /// Core organization apps
        /// 核心机构应用
        /// </summary>
        public ICollection<CoreOrganizationApp> CoreOrganizationApps { get; set; } = [];

        /// <summary>
        /// Core organization channel owners
        /// 核心机构渠道所有者
        /// </summary>
        public ICollection<CoreOrganizationChannel> CoreOrganizationChannelOwners { get; set; } = [];

        /// <summary>
        /// Core organization channel partners
        /// 核心机构渠道合作伙伴
        /// </summary>
        public ICollection<CoreOrganizationChannel> CoreOrganizationChannelPartners { get; set; } = [];

        /// <summary>
        /// Core organization users
        /// 核心机构用户
        /// </summary>
        public ICollection<CoreOrganizationUser> CoreOrganizationUsers { get; set; } = [];

        /// <summary>
        /// Core organization inverse parent
        /// 核心机构反向父机构
        /// </summary>
        public ICollection<CoreOrganization> InverseParent { get; set; } = [];

        /// <summary>
        /// Owner
        /// 所有者
        /// </summary>
        public CoreUser Owner { get; set; } = default!;

        /// <summary>
        /// Parent organization
        /// 父机构
        /// </summary>
        public CoreOrganization? Parent { get; set; }
    }
}
