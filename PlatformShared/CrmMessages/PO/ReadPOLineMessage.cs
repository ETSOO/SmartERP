using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Read purchase order line message
    /// 读取采购订单行消息
    /// </summary>
    public record ReadPOLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ReadPOLine";
    }
}
