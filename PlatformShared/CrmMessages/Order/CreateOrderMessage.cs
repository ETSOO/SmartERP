using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Create order message
    /// 创建订单消息
    /// </summary>
    public record CreateOrderMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateOrder";
    }
}
