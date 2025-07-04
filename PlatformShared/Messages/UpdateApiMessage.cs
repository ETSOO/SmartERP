using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Update API message
    /// 更新接口消息
    /// </summary>
    public record UpdateApiMessage : CommonUpdateMessage, IMessageQueueMessage
    {
        public static string Type => "UpdateApi";

        /// <summary>
        /// API organization id
        /// 接口所在机构编号
        /// </summary>
        public required int OrganizationId { get; init; }
    }
}
