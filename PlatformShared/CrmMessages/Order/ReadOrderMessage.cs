using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Read order message
    /// 读取订单消息
    /// </summary>
    public record ReadOrderMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ReadOrder";
    }
}
