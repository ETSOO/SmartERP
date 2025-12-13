namespace PlatformShared.Dto
{
    /// <summary>
    /// Common Email template view
    /// 通用电子邮件模板视图
    /// </summary>
    public record CommonEmailTemplateView
    {
        /// <summary>
        /// Language
        /// 语言
        /// </summary>
        public required string Language { get; init; }

        /// <summary>
        /// Email subject
        /// 邮件主题
        /// </summary>
        public string? Subject { get; set; }
    }
}
