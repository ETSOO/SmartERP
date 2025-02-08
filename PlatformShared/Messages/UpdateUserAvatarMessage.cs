using com.etsoo.MessageQueue;

namespace PlatformShared.Messages
{
    /// <summary>
    /// User update avatar message
    /// 用户更新头像消息
    /// </summary>
    public record UpdateUserAvatarMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateUserAvatar";
    }
}
