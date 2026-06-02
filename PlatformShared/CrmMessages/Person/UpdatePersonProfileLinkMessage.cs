using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Update person profile link message
    /// 更新人员档案链接消息
    /// </summary>
    public record UpdatePersonProfileLinkMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdatePersonProfileLink";
    }
}
