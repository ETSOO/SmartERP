using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Create API message
    /// 创建接口消息
    /// </summary>
    public record CreateApiMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateApi";

        /// <summary>
        /// API organization id
        /// 接口所在机构编号
        /// </summary>
        public required int OrganizationId { get; init; }
    }
}
