using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Update stock message
    /// 更新库存消息
    /// </summary>
    public record UpdateStockMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateStock";
    }
}
