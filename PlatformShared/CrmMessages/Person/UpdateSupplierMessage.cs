using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Update supplier message
    /// 更新供应商消息
    /// </summary>
    public record UpdateSupplierMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateSupplier";
    }
}
