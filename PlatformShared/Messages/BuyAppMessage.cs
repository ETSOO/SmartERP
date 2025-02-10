using com.etsoo.MessageQueue;
using System.Text.Json;

namespace PlatformShared.Messages
{
    public record BuyAppMessageData
    {
        public required int Months { get; init; }
        public required int OrgId { get; init; }
        public required bool NewOrg { get; init; }
    }

    /// <summary>
    /// Buy app message
    /// 购买应用消息
    /// </summary>
    public record BuyAppMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "BuyApp";

        /// <summary>
        /// Months
        /// 月数
        /// </summary>
        public required int Months { get; init; }

        /// <summary>
        /// Organization ID
        /// 机构编号
        /// </summary>
        public required int OrgId { get; init; }

        /// <summary>
        /// New organization or not
        /// 是否新机构
        /// </summary>
        public required bool NewOrg { get; init; }

        public override string? GetMoreData()
        {
            return JsonSerializer.Serialize(new BuyAppMessageData
            {
                Months = Months,
                OrgId = OrgId,
                NewOrg = NewOrg
            }, PlatformSharedContext.Default.BuyAppMessageData);
        }
    }
}
