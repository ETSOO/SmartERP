using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Admin create document message
    /// 管理员创建文档消息
    /// </summary>
    public record AdminCreateDocumentMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "AdminCreateDocument";
    }
}
