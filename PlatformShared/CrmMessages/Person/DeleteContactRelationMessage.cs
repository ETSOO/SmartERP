using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Delete contact relation message
    /// 删除联系人关系消息
    /// </summary>
    public record DeleteContactRelationMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeleteContactRelation";
    }
}
