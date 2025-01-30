using com.etsoo.MessageQueue;
using System.Text.Json;

namespace PlatformShared.Messages
{
    public record LoginFailedMessageData
    {
        public string? Reason { get; init; }
    }

    /// <summary>
    /// Login failed message
    /// 登录失败消息
    /// </summary>
    public record LoginFailedMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "LoginFailed";

        /// <summary>
        /// Reason
        /// 原因
        /// </summary>
        public string? Reason { get; init; }

        public override string? GetMoreData()
        {
            return JsonSerializer.Serialize(new LoginFailedMessageData
            {
                Reason = Reason
            }, PlatformSharedContext.Default.LoginFailedMessageData);
        }
    }
}
