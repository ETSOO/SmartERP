using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Admin merge accounts message
    /// 管理员合并账户消息
    /// </summary>
    public record AdminMergeAccountsMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "AdminMergeAccounts";

        /// <summary>
        /// Source user id
        /// 保留的用户编号
        /// </summary>
        public int SourceUserId { get; init; }

        /// <summary>
        /// Source user name
        /// 保留的用户姓名
        /// </summary>
        public required string SourceUserName { get; init; }

        /// <summary>
        /// Merged user id
        /// 合并的用户编号
        /// </summary>
        public int MergedUserId { get; init; }

        /// <summary>
        /// Merged user name
        /// 合并的用户姓名
        /// </summary>
        public required string MergedUserName { get; init; }

        /// <summary>
        /// Description
        /// 描述
        /// </summary>
        public required string Description { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(SourceUserId)] = SourceUserId,
            [nameof(SourceUserName)] = SourceUserName,
            [nameof(MergedUserId)] = MergedUserId,
            [nameof(MergedUserName)] = MergedUserName,
            [nameof(Description)] = Description
        };
    }
}
