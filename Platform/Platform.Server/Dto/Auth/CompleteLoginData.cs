using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;

namespace Platform.Server.Dto.Auth
{
    /// <summary>
    /// Complete login result data
    /// 完成登录结果数据
    /// </summary>
    public record CompleteLoginData
    {
        /// <summary>
        /// Verified organization id
        /// 认证的机构编号
        /// </summary>
        public int? OrganizationId { get; init; }

        /// <summary>
        /// Organization name
        /// 机构名称
        /// </summary>
        public string? OrganizationName { get; init; }

        /// <summary>
        /// Parent organization id
        /// 父机构编号
        /// </summary>
        public int? ParentOrganizationId { get; init; }

        /// <summary>
        /// Channel organization id
        /// 渠道机构编号
        /// </summary>
        public int? ChannelOrganizationId { get; init; }

        /// <summary>
        /// Device id
        /// 设备编号
        /// </summary>
        public int DeviceId { get; init; }

        /// <summary>
        /// Organization user id
        /// 机构用户编号
        /// </summary>
        public int? Oid { get; init; }

        /// <summary>
        /// User global unique identifier
        /// 用户全局唯一标识符
        /// </summary>
        public Guid? Uid { get; init; }

        /// <summary>
        /// Organization entity status
        /// 机构实体状态
        /// </summary>
        public EntityStatus? OrgStatus { get; init; }

        /// <summary>
        /// Organization entity expiry
        /// 机构实体到期
        /// </summary>
        public DateTimeOffset? OrgExpiry { get; init; }

        /// <summary>
        /// User role
        /// 用户角色
        /// </summary>
        public UserRole UserRole { get; init; }

        /// <summary>
        /// Local name
        /// 本地姓名
        /// </summary>
        public string? LocalName { get; init; }

        /// <summary>
        /// Local avatar
        /// 本地头像
        /// </summary>
        public string? LocalAvatar { get; init; }

        /// <summary>
        /// Permission scopes
        /// 权限范围
        /// </summary>
        public int[] Scopes { get; init; } = [];
    }
}
