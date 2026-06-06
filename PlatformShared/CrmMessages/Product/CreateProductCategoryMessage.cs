using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Create product category message
    /// 创建产品类目消息
    /// </summary>
    public record CreateProductCategoryMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateProductCategory";
    }
}
