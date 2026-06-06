using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Stock take message
    /// 库存盘点消息
    /// </summary>
    public record StockTakeMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "StockTake";
    }
}
