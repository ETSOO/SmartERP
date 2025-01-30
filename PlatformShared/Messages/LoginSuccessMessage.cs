using com.etsoo.MessageQueue;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Login success message
    /// 成功登录消息
    /// </summary>
    public record LoginSuccessMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "LoginSuccess";
    }
}
