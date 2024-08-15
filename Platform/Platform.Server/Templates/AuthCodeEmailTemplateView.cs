using Platform.Server.Dto.Auth;

namespace Platform.Server.Templates
{
    /// <summary>
    /// Authorization code email template view model
    /// 验证码邮件模板浏览模型
    /// </summary>
    public record AuthCodeEmailTemplateView : CommonEmailTemplateView
    {
        /// <summary>
        /// Action data
        /// 操作数据
        /// </summary>
        public required AuthCodeAction Action { get; init; }

        /// <summary>
        /// Authorization code
        /// 验证码
        /// </summary>
        public required string Code { get; init; }

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
    }
}
