using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Update person address message
    /// 更新人员地址消息
    /// </summary>
    public record UpdatePersonAddressMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdatePersonAddress";
    }
}
