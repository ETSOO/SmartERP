using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Delete order message
    /// 移除订单消息
    /// </summary>
    public record DeleteOrderMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeleteOrder";
    }
}
