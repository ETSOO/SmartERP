using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Rollback order line message
    /// 回滚订单行消息
    /// </summary>
    public record RollbackOrderLineMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "RollbackOrderLine";
    }
}
