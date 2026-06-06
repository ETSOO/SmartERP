using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Delete stock message
    /// 移除库存消息
    /// </summary>
    public record DeleteStockMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeleteStock";
    }
}
