using com.etsoo.MessageQueue;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Update person profile message
    /// 更新人员档案消息
    /// </summary>
    public record UpdatePersonProfileMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdatePersonProfile";
    }
}