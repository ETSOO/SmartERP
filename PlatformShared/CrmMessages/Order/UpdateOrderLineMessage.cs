using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Update order line message
    /// 更新订单行消息
    /// </summary>
    public record UpdateOrderLineMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateOrderLine";
    }
}
