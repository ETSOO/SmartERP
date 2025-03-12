using com.etsoo.CoreFramework.Authentication;

namespace Platform.Server.Endpoints.Member.RQ
{
    /// <summary>
    /// Member list request data
    /// 成员列表请求数据
    /// </summary>
    public record MemberListRQ : QueryLongRQ
    {
        /// <summary>
        /// Exclude current user or not
        /// 是否排除当前用户
        /// </summary>
        public bool? ExcludeSelf { get; init; }

        /// <summary>
        /// User role
        /// 用户角色
        /// </summary>
        public UserRole? UserRole { get; init; }

        /// <summary>
        /// User role start
        /// 用户角色起始
        /// </summary>
        public UserRole? UserRoleStart { get; init; }

        /// <summary>
        /// Inviter id
        /// 邀请人编号
        /// </summary>
        public int? InviterId { get; init; }

        /// <summary>
        /// Report to
        /// 汇报对象
        /// </summary>
        public int? ReportTo { get; init; }
    }
}
