using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Stock create line message
    /// 库存创建行消息
    /// </summary>
    public record StockCreateLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "StockCreateLine";
    }
}
