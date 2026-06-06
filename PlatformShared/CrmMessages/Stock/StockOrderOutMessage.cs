using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Stock order out message
    /// 订单发货消息
    /// </summary>
    public record StockOrderOutMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "StockOrderOut";
    }
}
