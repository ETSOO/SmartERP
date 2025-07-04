using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Create resource message
    /// 创建资源消息
    /// </summary>
    public record CreateResourceMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "CreateResource";

        /// <summary>
        /// Request data
        /// 请求数据
        /// </summary>
        public required string RequestData { get; init; }

        public override string? GetMoreData()
        {
            return RequestData;
        }
    }
}
