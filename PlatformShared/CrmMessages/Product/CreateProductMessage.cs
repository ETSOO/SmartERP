using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Create product message
    /// 创建产品消息
    /// </summary>
    public record CreateProductMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateProduct";
    }
}
