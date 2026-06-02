using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Create person profile message
    /// 创建人员档案消息
    /// </summary>
    public record CreatePersonProfileMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreatePersonProfile";
    }
}
