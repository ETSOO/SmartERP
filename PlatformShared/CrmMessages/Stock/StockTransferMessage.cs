using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Stock transfer message
    /// 库存调货消息
    /// </summary>
    public record StockTransferMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "StockTransfer";
    }
}
