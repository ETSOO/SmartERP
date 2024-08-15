using com.etsoo.CoreFramework.Authentication;

namespace Platform.Server.Dto.Auth
{
    /// <summary>
    /// Complete login result data
    /// 完成登录结果数据
    /// </summary>
    public record CompleteLoginData
    {
        /// <summary>
        /// Test organization id
        /// 测试机构编号
        /// </summary>
        public int? TestOrganizationId { get; init; }

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
        /// User role
        /// 用户角色
        /// </summary>
        public UserRole UserRole { get; init; }

        /// <summary>
        /// Permission scopes
        /// 权限范围
        /// </summary>
        public short[] Scopes { get; init; } = [];
    }
}
