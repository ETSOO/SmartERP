using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Create order delivery message
    /// 创建订单配送方式消息
    /// </summary>
    public record CreateOrderDeliveryMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateOrderDelivery";
    }
}
