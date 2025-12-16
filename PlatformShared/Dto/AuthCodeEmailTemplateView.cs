using System.Text.Json.Serialization;

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
        /// Time zone ID
        /// 时区编号
        /// </summary>
        public required string TimeZoneId { get; init; }

        /// <summary>
        /// Time zone
        /// 时区
        /// </summary>
        [JsonIgnore]
        public TimeZoneInfo TimeZone
        {
            get => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }

        /// <summary>
        /// Local expiry
        /// 本地过期时间
        /// </summary>
        public required DateTime LocalExpiry { get; init; }

        /// <summary>
        /// JSON Data
        /// JSON 数据
        /// </summary>
        public string? Data { get; init; }
    }
}
