using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Sort order payment message
    /// 排序订单支付方式消息
    /// </summary>
    public record SortOrderPaymentMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "SortOrderPayment";
    }
}
