using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Delete purchase order message
    /// 移除采购订单消息
    /// </summary>
    public record DeletePOMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeletePO";
    }
}
