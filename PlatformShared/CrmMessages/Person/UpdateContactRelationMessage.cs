using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Update contact relation message
    /// 更新联系人关系消息
    /// </summary>
    public record UpdateContactRelationMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateContactRelation";
    }
}
