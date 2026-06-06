using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Create order payment message
    /// 创建订单支付方式消息
    /// </summary>
    public record CreateOrderPaymentMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateOrderPayment";
    }
}
