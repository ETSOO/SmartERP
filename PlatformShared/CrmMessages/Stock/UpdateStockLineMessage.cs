using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Stock
{
    /// <summary>
    /// Update stock line message
    /// 更新库存行消息
    /// </summary>
    public record UpdateStockLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateStockLine";
    }
}
