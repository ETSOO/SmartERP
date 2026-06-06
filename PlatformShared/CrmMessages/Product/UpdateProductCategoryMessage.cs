using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Update product category message
    /// 更新产品类目消息
    /// </summary>
    public record UpdateProductCategoryMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateProductCategory";
    }
}
