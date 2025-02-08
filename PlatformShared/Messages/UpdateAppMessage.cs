using com.etsoo.MessageQueue;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Update app message
    /// 更新应用消息
    /// </summary>
    public record UpdateAppMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateApp";
    }
}
