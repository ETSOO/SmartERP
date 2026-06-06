using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Create order line message
    /// 创建订单行消息
    /// </summary>
    public record CreateOrderLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateOrderLine";
    }
}
