using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Create purchase order message
    /// 创建采购订单消息
    /// </summary>
    public record CreatePOMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreatePO";
    }
}
