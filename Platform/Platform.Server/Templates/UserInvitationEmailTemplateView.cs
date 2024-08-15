using Platform.Server.Dto.User;

namespace Platform.Server.Templates
{
    /// <summary>
    /// User invitation email template view
    /// 用户邀请电子邮件模板视图
    /// </summary>
    public record UserInvitationEmailTemplateView : CommonEmailTemplateView
    {
        /// <summary>
        /// Time zone
        /// 时区
        /// </summary>
        public required TimeZoneInfo TimeZone { get; init; }

        /// <summary>
        /// Local expiry
        /// 本地过期时间
        /// </summary>
        public required DateTime LocalExpiry { get; init; }

        /// <summary>
        /// User data
        /// 用户信息
        /// </summary>
        public required UserData UserData { get; init; }

        /// <summary>
        /// Unique identifier
        /// 唯一编号
        /// </summary>
        public required Guid Guid { get; init; }

        /// <summary>
        /// Web URL to access
        /// 访问的网络地址
        /// </summary>
        public required string WebUrl { get; init; }

        /// <summary>
        /// Additional message
        /// 附加信息
        /// </summary>
        public string? Message { get; init; }
    }
}
