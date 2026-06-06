using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Create promotion message
    /// 创建促销消息
    /// </summary>
    public record CreatePromotionMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreatePromotion";
    }
}
