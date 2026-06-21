using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Recalculate order message
    /// 重新计算订单消息
    /// </summary>
    public record RecalculateOrderMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "RecalculateOrder";

        /// <summary>
        /// Order creation
        /// 订单登记时间
        /// </summary>
        public DateTimeOffset OrderCreation { get; init; }
    }
}
