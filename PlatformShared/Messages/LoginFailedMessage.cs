using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Login failed message
    /// 登录失败消息
    /// </summary>
    public record LoginFailedMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "LoginFailed";

        /// <summary>
        /// Login failed clear type
        /// 登录失败清除类型
        /// </summary>
        public const string ClearType = "LoginFailedClear";

        /// <summary>
        /// Reason
        /// 原因
        /// </summary>
        public string? Reason { get; init; }

        /// <summary>
        /// User agent
        /// 用户代理
        /// </summary>
        public string? UserAgent { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(Reason)] = Reason,
            [nameof(UserAgent)] = UserAgent,
            [nameof(Data.TimeZone)] = Data.TimeZone
        };
    }
}
