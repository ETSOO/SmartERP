using com.etsoo.Utils.Serialization;
using System.Text.Json;

namespace PlatformShared.Messages
{
    public record AdjustReportToMessageData
    {
        public required int Count { get; init; }
        public required int NewReportTo { get; init; }
        public required string NewReportToName { get; init; }
    }

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

        public override string? GetMoreData()
        {
            return JsonSerializer.Serialize(new AdjustReportToMessageData
            {
                Count = Count,
                NewReportTo = NewReportTo,
                NewReportToName = NewReportToName
            }, PlatformSharedContext.Default.AdjustReportToMessageData);
        }
    }
}
