using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Start order line execution message
    /// 开始订单行执行消息
    /// </summary>
    public record StartOrderLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "StartOrderLine";
    }
}
