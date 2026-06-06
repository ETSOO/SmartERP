using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Complete order line message
    /// 完成订单行消息
    /// </summary>
    public record CompleteOrderLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CompleteOrderLine";
    }
}
