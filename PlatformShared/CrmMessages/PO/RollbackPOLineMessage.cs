using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Rollback purchase order line message
    /// 回滚采购订单行消息
    /// </summary>
    public record RollbackPOLineMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "RollbackPOLine";
    }
}
