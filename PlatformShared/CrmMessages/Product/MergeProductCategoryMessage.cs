using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Product
{
    /// <summary>
    /// Merge product category message
    /// 合并产品类目消息
    /// </summary>
    public record MergeProductCategoryMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "MergeProductCategory";

        /// <summary>
        /// Delete the source or not
        /// 是否删除源类别
        /// </summary>
        public bool? DeleteSource { get; init; }

        /// <summary>
        /// Source id
        /// 源编号
        /// </summary>
        public int SourceId { get; init; }

        /// <summary>
        /// Source name
        /// 源名称
        /// </summary>
        public required string SourceName { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(DeleteSource)] = DeleteSource,
            [nameof(SourceId)] = SourceId,
            [nameof(SourceName)] = SourceName
        };
    }
}
