using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Batch adjusting report to message
    /// 批量调整汇报对象消息
    /// </summary>
    public record AdjustReportToMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "AdjustReportTo";

        /// <summary>
        /// Count
        /// 计数
        /// </summary>
        public required int Count { get; init; }

        /// <summary>
        /// New report to
        /// 新汇报对象
        /// </summary>
        public required int NewReportTo { get; init; }

        /// <summary>
        /// New report to name
        /// 新汇报对象姓名
        /// </summary>
        public required string NewReportToName { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(Count)] = Count,
            [nameof(NewReportTo)] = NewReportTo,
            [nameof(NewReportToName)] = NewReportToName
        };
    }
}
