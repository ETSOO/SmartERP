using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Update user self message
    /// 更新用户本人消息
    /// </summary>
    public record UpdateUserSelfMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateUserSelf";
    }
}
