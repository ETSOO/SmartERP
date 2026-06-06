using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Read order line message
    /// 读取订单行消息
    /// </summary>
    public record ReadOrderLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ReadOrderLine";
    }
}
