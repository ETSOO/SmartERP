using com.etsoo.Utils.Serialization;

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

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(UserAgent)] = UserAgent,
            [nameof(Data.TimeZone)] = Data.TimeZone
        };
    }
}
