using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Delete document message
    /// 删除文档消息
    /// </summary>
    public record DeleteDocumentMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeleteDocument";

        /// <summary>
        /// Document organization id
        /// 文档所在机构编号
        /// </summary>
        public int? OrganizationId { get; init; }
    }
}
