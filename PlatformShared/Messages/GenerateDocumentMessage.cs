using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Generate document message
    /// 输出文档消息
    /// </summary>
    public record GenerateDocumentMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "GenerateDocument";

        /// <summary>
        /// Culture
        /// 文化
        /// </summary>
        public required string Culture { get; init; }

        /// <summary>
        /// Target id
        /// 目标编号
        /// </summary>
        public long TargetId { get; init; }

        /// <summary>
        /// Target name
        /// 目标对象名称
        /// </summary>
        public required string TargetName { get; init; }

        /// <summary>
        /// More parameters
        /// 更多参数
        /// </summary>
        public string? Parameters { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(Culture)] = Culture,
            [nameof(TargetId)] = TargetId,
            [nameof(TargetName)] = TargetName,
            [nameof(Parameters)] = Parameters
        };
    }
}
