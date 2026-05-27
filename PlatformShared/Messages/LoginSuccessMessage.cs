using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
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

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(UserAgent)] = UserAgent,
            [nameof(Data.TimeZone)] = Data.TimeZone
        };
    }
}
