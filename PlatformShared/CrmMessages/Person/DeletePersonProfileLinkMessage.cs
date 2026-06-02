using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Delete person profile link message
    /// 删除人员档案链接消息
    /// </summary>
    public record DeletePersonProfileLinkMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeletePersonProfileLink";
    }
}
