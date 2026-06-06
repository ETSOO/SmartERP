using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Stock receive message
    /// 入库消息
    /// </summary>
    public record StockReceiveMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "StockReceive";
    }
}
