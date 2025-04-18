using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Update member avatar message
    /// 更新成员头像消息
    /// </summary>
    public record UpdateMemberAvatarMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateMemberAvatar";
    }
}
