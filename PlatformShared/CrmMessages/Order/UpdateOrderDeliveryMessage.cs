using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Update order delivery message
    /// 更新订单配送方式消息
    /// </summary>
    public record UpdateOrderDeliveryMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateOrderDelivery";
    }
}
