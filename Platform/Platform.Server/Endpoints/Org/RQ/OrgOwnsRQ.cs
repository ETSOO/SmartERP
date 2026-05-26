using com.etsoo.CoreFramework.Authentication;

namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Check organization ownership request data
    /// 检查机构所有权请求数据
    /// </summary>
    public record OrgOwnsRQ
    {
        /// <summary>
        /// Org id
        /// 机构编号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Minimum role
        /// 最低角色
        /// </summary>
        public UserRole MinRole { get; init; }
    }
}
