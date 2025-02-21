using com.etsoo.MessageQueue;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Check app session message
    /// 检查应用会话消息
    /// </summary>
    public record CheckSessionMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CheckSession";
    }
}
