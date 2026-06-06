using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Update purchase order line message
    /// 更新采购订单行消息
    /// </summary>
    public record UpdatePOLineMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdatePOLine";
    }
}
