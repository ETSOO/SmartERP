using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Stock PO in message
    /// 采购入库消息
    /// </summary>
    public record StockPOInMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "StockPOIn";
    }
}
