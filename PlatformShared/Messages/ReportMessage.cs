using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Report message
    /// 报表消息
    /// </summary>
    public record ReportMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "Report";

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Query parameters
        /// 查询参数
        /// </summary>
        public Dictionary<string, object?>? Parameters { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new(Parameters ?? [])
        {
            [nameof(Title)] = Title
        };
    }
}
