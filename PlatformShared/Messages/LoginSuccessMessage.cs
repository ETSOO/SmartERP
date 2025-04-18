using com.etsoo.Utils.Serialization;
using System.Text.Json;

namespace PlatformShared.Messages
{
    public record LoginSuccessMessageData
    {
        public string? UserAgent { get; init; }
        public required string TimeZone { get; init; }
    }

    /// <summary>
    /// Login success message
    /// 成功登录消息
    /// </summary>
    public record LoginSuccessMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "LoginSuccess";

        /// <summary>
        /// User agent
        /// 用户代理
        /// </summary>
        public string? UserAgent { get; init; }

        public override string? GetMoreData()
        {
            return JsonSerializer.Serialize(new LoginSuccessMessageData
            {
                UserAgent = UserAgent,
                TimeZone = Data.TimeZone
            }, PlatformSharedContext.Default.LoginSuccessMessageData);
        }
    }
}
