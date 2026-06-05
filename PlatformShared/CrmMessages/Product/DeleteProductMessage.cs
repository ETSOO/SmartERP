using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Delete product message
    /// 移除产品消息
    /// </summary>
    public record DeleteProductMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeleteProduct";
    }
}
