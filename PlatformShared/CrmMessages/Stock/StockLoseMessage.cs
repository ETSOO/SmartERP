using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Stock lose message
    /// 库存报损消息
    /// </summary>
    public record StockLoseMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "StockLose";
    }
}
