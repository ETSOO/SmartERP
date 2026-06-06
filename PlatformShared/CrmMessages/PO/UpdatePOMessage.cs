using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Update purchase order message
    /// 更新采购订单消息
    /// </summary>
    public record UpdatePOMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdatePO";
    }
}
