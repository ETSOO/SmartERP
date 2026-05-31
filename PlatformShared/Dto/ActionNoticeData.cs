using com.etsoo.Database.Converters;
using PlatformShared.Messages;
using System.Diagnostics.CodeAnalysis;

namespace PlatformShared.Dto
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

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="data">Message data</param>
        /// <param name="subject">Subject</param>
        /// <param name="action">Action</param>
        /// <param name="detail">Detail</param>
        [SetsRequiredMembers]
        public ActionNoticeData(CommonMessageData data, string subject, string action, string? detail = null)
        {
            Language = data.Culture;
            Subject = subject;
            UserName = data.UserName;
            Action = action;
            Detail = detail;
            IP = data.IP;
            TimeZone = data.TimeZone;
            TimeStamp = data.TimeStamp;
        }

        /// <summary>
        /// Format notice date time
        /// 格式化通知日期时间
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string FormatDateTime()
        {
            var localTime = TimeZoneInfo.ConvertTime(TimeStamp, TZ);
            return $"{localTime} ({TZ.StandardName})";
        }
    }
}
