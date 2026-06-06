using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Stock assemble message
    /// 库存组装消息
    /// </summary>
    public record StockAssembleMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "StockAssemble";
    }
}
