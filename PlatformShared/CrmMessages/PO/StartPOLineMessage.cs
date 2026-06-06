using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.PO
{
    /// <summary>
    /// Start purchase order line execution message
    /// 开始采购订单行执行消息
    /// </summary>
    public record StartPOLineMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "StartPOLine";
    }
}
