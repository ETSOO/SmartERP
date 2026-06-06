using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Delete order line message
    /// 移除订单行消息
    /// </summary>
    public record DeleteOrderLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeleteOrderLine";
    }
}
