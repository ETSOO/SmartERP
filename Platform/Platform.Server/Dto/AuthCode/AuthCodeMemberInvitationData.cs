using com.etsoo.CoreFramework.Authentication;
using Platform.Server.Dto.User;

namespace Platform.Server.Dto.AuthCode
{
    /// <summary>
    /// Member invitation auth code data
    /// 成员邀请验证码数据
    /// </summary>
    public record AuthCodeMemberInvitationData : AuthCodeData
    {
        /// <summary>
        /// User data
        /// 用户信息
        /// </summary>
        public required UserData UserData { get; init; }

        /// <summary>
        /// Web URL to access
        /// 访问的网络地址
        /// </summary>
        public required string WebUrl { get; init; }

        /// <summary>
        /// User role
        /// 用户角色
        /// </summary>
        public required UserRole UserRole { get; init; }

        /// <summary>
        /// Additional message
        /// 附加信息
        /// </summary>
        public string? Message { get; init; }
    }
}
