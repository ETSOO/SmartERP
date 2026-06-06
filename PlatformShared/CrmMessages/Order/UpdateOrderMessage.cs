using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Update order message
    /// 更新订单消息
    /// </summary>
    public record UpdateOrderMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateOrder";
    }
}
