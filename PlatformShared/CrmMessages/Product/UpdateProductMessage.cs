using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Update product message
    /// 更新产品消息
    /// </summary>
    public record UpdateProductMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateProduct";
    }
}
