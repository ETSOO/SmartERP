using com.etsoo.Utils.Serialization;
using System.Text.Json;

namespace PlatformShared.Messages
{
    public record LoginFailedMessageData
    {
        public string? Reason { get; init; }
        public string? UserAgent { get; init; }
        public required string TimeZone { get; init; }
    }

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

        public override string? GetMoreData()
        {
            return JsonSerializer.Serialize(new LoginFailedMessageData
            {
                Reason = Reason,
                UserAgent = UserAgent,
                TimeZone = Data.TimeZone
            }, PlatformSharedContext.Default.LoginFailedMessageData);
        }
    }
}
