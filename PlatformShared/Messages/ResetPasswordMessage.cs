using com.etsoo.Utils.Serialization;
using System.Text.Json;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Reset password message
    /// 重置密码消息
    /// </summary>
    public record ResetPasswordMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ResetPassword";

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
