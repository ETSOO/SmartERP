using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Update organization avatar message
    /// 更新机构头像消息
    /// </summary>
    public record UpdateOrgAvatarMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateOrgAvatar";
    }
}
