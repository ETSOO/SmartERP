using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Update person product message
    /// 更新人员个性化产品消息
    /// </summary>
    public record UpdatePersonProductMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdatePersonProduct";
    }
}
