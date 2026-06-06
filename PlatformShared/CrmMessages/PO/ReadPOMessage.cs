using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Read purchase order message
    /// 读取采购订单消息
    /// </summary>
    public record ReadPOMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ReadPO";
    }
}
