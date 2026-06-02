using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Create person profile link message
    /// 创建人员档案链接消息
    /// </summary>
    public record CreatePersonProfileLinkMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreatePersonProfileLink";
    }
}
