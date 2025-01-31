using com.etsoo.Database.Converters;

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
        /// Detail
        /// 细节
        /// </summary>
        public string? Detail { get; init; }

        /// <summary>
        /// IP
        /// 网络地址
        /// </summary>
        public required string IP { get; init; }

        /// <summary>
        /// Time zone
        /// 时区
        /// </summary>
        public required string TimeZone { get; init; }

        /// <summary>
        /// Time stamp
        /// 时间戳
        /// </summary>
        public required DateTimeOffset TimeStamp { get; init; }

        private TimeZoneInfo? _tz;

        /// <summary>
        /// Time zone info
        /// 时区信息
        /// </summary>
        public TimeZoneInfo TZ
        {
            get
            {
                _tz ??= TimeZoneUtils.GetTimeZone(TimeZone);
                return _tz;
            }
        }
    }
}
