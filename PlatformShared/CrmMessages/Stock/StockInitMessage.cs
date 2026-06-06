using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Stock init message
    /// 库存初始化消息
    /// </summary>
    public record StockInitMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "StockInit";
    }
}
