using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;

namespace PlatformShared.Database.Models
{
    /// <summary>
    /// Core organization's user
    /// 核心机构的用户
    /// </summary>
    public class CoreOrganizationUser
    {
        /// <summary>
        /// Organization user id
        /// 机构用户编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Global unique identifier
        /// 全局唯一标识符
        /// </summary>
        public Guid Uid { get; set; }

        /// <summary>
        /// Core organization id
        /// 核心机构编号
        /// </summary>
        public int CoreOrganizationId { get; set; }

        /// <summary>
        /// Core user id
        /// 核心用户编号
        /// </summary>
        public int CoreUserId { get; set; }

        /// <summary>
        /// User role
        /// 用户角色
        /// </summary>
        public UserRole UserRole { get; set; }

        /// <summary>
        /// Identity type
        /// 标识类型
        /// </summary>
        public IdentityTypeFlags IdentityType { get; set; }

        /// <summary>
        /// Local name
        /// 本地名称
        /// </summary>
        public string? LocalName { get; set; }

        /// <summary>
        /// Local avatar
        /// 本地头像
        /// </summary>
        public string? LocalAvatar { get; set; }

        /// <summary>
        /// Permission value
        /// 权限值
        /// </summary>
        public int? Permission { get; set; }

        /// <summary>
        /// Assigned id
        /// 分配的编号
        /// </summary>
        public string? AssignedId { get; set; }

        /// <summary>
        /// Extended data
        /// 扩展数据
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Creation
        /// 创建时间
        /// </summary>
        public DateTimeOffset Creation { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Expiry
        /// 过期时间
        /// </summary>
        public DateTimeOffset? Expiry { get; set; }

        /// <summary>
        /// Refresh time
        /// 刷新时间
        /// </summary>
        public DateTimeOffset RefreshTime { get; set; }

        /// <summary>
        /// Status
        /// 状态
        /// </summary>
        public EntityStatus Status { get; set; } = EntityStatus.Normal;

        /// <summary>
        /// Inviter id
        /// 邀请人编号
        /// </summary>
        public int? InviterId { get; set; }

        /// <summary>
        /// Core organization
        /// 核心机构
        /// </summary>
        public CoreOrganization CoreOrganization { get; set; } = default!;

        /// <summary>
        /// Core user
        /// 核心用户
        /// </summary>
        public CoreUser CoreUser { get; set; } = default!;

        /// <summary>
        /// Inviter
        /// 邀请人
        /// </summary>
        public CoreUser Inviter { get; set; } = default!;
    }
}
