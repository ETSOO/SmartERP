using com.etsoo.MessageQueue;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Change password message
    /// 修改密码消息
    /// </summary>
    public record ChangePasswordMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "ChangePassword";
    }
}