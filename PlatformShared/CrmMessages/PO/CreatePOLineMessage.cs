using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Create purchase order line message
    /// 创建采购订单行消息
    /// </summary>
    public record CreatePOLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreatePOLine";
    }
}
