using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Update product price message
    /// 更新产品价格消息
    /// </summary>
    public record UpdateProductPriceMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateProductPrice";
    }
}
