using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Delete document message
    /// 删除文档消息
    /// </summary>
    public record DeleteDocumentMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "DeleteDocument";
    }
}
