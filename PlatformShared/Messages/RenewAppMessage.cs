using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Renew app message
    /// 应用续费消息
    /// </summary>
    public record RenewAppMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "RenewApp";

        /// <summary>
        /// Months
        /// 月数
        /// </summary>
        public required int Months { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(Months)] = Months
        };
    }
}
