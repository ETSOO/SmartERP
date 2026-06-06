using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Recalculate purchase order message
    /// 重新计算采购订单消息
    /// </summary>
    public record RecalculatePOMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "RecalculatePO";
    }
}
