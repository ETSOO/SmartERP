using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Order
{
    /// <summary>
    /// Sort order delivery message
    /// 排序订单配送方式消息
    /// </summary>
    public record SortOrderDeliveryMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "SortOrderDelivery";
    }
}
