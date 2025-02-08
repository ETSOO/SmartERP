using com.etsoo.MessageQueue;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Create API key message
    /// 创建API密钥消息
    /// </summary>
    public record CreateApiKeyMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateApiKey";
    }
}
