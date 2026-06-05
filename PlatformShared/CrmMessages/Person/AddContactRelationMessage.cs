using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Add contact relation message
    /// 添加联系人关系消息
    /// </summary>
    public record AddContactRelationMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "AddContactRelation";
    }
}
