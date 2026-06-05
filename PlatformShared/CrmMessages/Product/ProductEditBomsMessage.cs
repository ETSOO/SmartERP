using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Product edit BOMs message
    /// 产品编辑BOM消息
    /// </summary>
    public record ProductEditBomsMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ProductEditBoms";
    }
}
