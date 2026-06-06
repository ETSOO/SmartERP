using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Sort product category message
    /// 排序产品类目消息
    /// </summary>
    public record SortProductCategoryMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "SortProductCategory";
    }
}
