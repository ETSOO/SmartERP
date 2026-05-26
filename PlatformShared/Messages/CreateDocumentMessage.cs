using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Create document message
    /// 创建文档消息
    /// </summary>
    public record CreateDocumentMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateDocument";
    }
}
