using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Update person message
    /// 更新人员信息
    /// </summary>
    public record UpdatePersonMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdatePerson";
    }
}
