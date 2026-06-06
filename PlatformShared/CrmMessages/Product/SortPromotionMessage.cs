using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Sort promotion message
    /// 排序促销消息
    /// </summary>
    public record SortPromotionMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "SortPromotion";
    }
}
