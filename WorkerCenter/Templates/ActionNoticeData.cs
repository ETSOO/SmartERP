namespace WorkerCenter.Templates
{
    /// <summary>
    /// Action notice data
    /// 操作通知数据
    /// </summary>
    public record ActionNoticeData
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
        public required string Subject { get; set; }

        /// <summary>
        /// User name
        /// 用户姓名
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Action
        /// 操作
        /// </summary>
        public required string Action { get; init; }

        /// <summary>
        /// IP
        /// 网络地址
        /// </summary>
        public required string IP { get; init; }
    }
}
