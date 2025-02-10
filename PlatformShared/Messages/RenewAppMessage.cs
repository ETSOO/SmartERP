using com.etsoo.MessageQueue;
using System.Text.Json;

namespace PlatformShared.Messages
{
    public record RenewAppMessageData
    {
        public required int Months { get; init; }
    }

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

        public override string? GetMoreData()
        {
            return JsonSerializer.Serialize(new RenewAppMessageData
            {
                Months = Months
            }, PlatformSharedContext.Default.RenewAppMessageData);
        }
    }
}
