using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Delete purchase order line message
    /// 移除采购订单行消息
    /// </summary>
    public record DeletePOLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeletePOLine";
    }
}
