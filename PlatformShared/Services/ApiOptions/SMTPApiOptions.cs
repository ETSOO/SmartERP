namespace PlatformShared.Services.ApiOptions
{
    /// <summary>
    /// SMTP API options
    /// 邮件接口选项
    /// </summary>
    public record SMTPApiOptions
    {
        /// <summary>
        /// CC recipients
        /// 抄送收件人
        /// </summary>
        public string[]? Cc { get; init; }

        /// <summary>
        /// BCC recipients
        /// 密送收件人
        /// </summary>
        public string[]? Bcc { get; init; }
    }
}
