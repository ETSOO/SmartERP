using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Update member message
    /// 更新成员消息
    /// </summary>
    public record UpdateMemberMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateMember";
    }
}
