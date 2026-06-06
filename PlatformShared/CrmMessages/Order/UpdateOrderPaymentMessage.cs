using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Update order payment message
    /// 更新订单支付方式消息
    /// </summary>
    public record UpdateOrderPaymentMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateOrderPayment";
    }
}
