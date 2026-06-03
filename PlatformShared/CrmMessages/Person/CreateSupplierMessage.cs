using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Create supplier message
    /// 创建供应商消息
    /// </summary>
    public record CreateSupplierMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateSupplier";
    }
}
