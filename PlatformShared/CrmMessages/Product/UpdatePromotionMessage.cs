using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Update promotion message
    /// 更新促销消息
    /// </summary>
    public record UpdatePromotionMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdatePromotion";
    }
}
