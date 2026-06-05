using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Serialization;
using PlatformShared.Messages;

namespace PlatformShared.CrmMessages.Person
{
    /// <summary>
    /// Merge person category message
    /// 合并人员类别消息
    /// </summary>
    public record MergePersonCategoryMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "MergePersonCategory";

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

        /// <summary>
        /// Identity type, employee, customer, or supplier
        /// 标识类型，员工、客户或供应商
        /// </summary>
        public IdentityTypeFlags IdentityType { get; set; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(SourceId)] = SourceId,
            [nameof(SourceName)] = SourceName,
            [nameof(IdentityType)] = IdentityType.ToString()
        };
    }
}