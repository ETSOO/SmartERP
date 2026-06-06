using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Complete purchase order line message
    /// 完成采购订单行消息
    /// </summary>
    public record CompletePOLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CompletePOLine";
    }
}
