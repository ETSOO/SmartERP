using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Update document message
    /// 更新文档消息
    /// </summary>
    public record UpdateDocumentMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateDocument";
    }
}
