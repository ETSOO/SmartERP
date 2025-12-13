using System.Diagnostics.CodeAnalysis;

namespace PlatformShared.Dto
{
    /// <summary>
    /// Authorization code email template view model
    /// 验证码邮件模板浏览模型
    /// </summary>
    public record AuthCodeEmailTemplateView : CommonEmailTemplateView
    {
        /// <summary>
        /// Id
        /// 编号
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// Action data
        /// 操作数据
        /// </summary>
        public required AuthCodeActionItem Action { get; init; }

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

    /// <summary>
    /// Authorization code email template view model
    /// 验证码邮件模板浏览模型
    /// </summary>
    public record AuthCodeEmailTemplateView<D> : AuthCodeEmailTemplateView where D : AuthCodeData
    {
        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public virtual required D Data { get; init; }

        [SetsRequiredMembers]
        public AuthCodeEmailTemplateView(AuthCodeEmailTemplateView view, D data) : base(view)
        {
            Data = data;
        }
    }
}
