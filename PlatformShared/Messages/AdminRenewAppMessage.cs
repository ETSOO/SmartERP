using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Admin renew app message
    /// 管理员应用续费消息
    /// </summary>
    public record AdminRenewAppMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "AdminRenewApp";

        /// <summary>
        /// Months
        /// 月数
        /// </summary>
        public required int Months { get; init; }

        /// <summary>
        /// Requester
        /// 请求人
        /// </summary>
        public required int Requester { get; init; }

        /// <summary>
        /// Requester local id
        /// 请求人本地编号
        /// </summary>
        public required long RequesterLocalId { get; init; }

        /// <summary>
        /// Requester organization id
        /// 请求人机构编号
        /// </summary>
        public required int RequesterOrgId { get; init; }

        /// <summary>
        /// Requester organization name
        /// 请求人机构名称
        /// </summary>
        public required string RequesterOrgName { get; init; }

        /// <summary>
        /// Approver
        /// 批准人
        /// </summary>
        public required int Approver { get; init; }

        /// <summary>
        /// Approver local id
        /// 批准人本地编号
        /// </summary>
        public required long ApproverLocalId { get; init; }

        /// <summary>
        /// Comment
        /// 备注
        /// </summary>
        public required string Comment { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(Months)] = Months,
            [nameof(Comment)] = Comment,
            [nameof(ApproverLocalId)] = ApproverLocalId,
            [nameof(Requester)] = Requester,
            [nameof(RequesterLocalId)] = RequesterLocalId,
            [nameof(RequesterOrgName)] = RequesterOrgName
        };
    }
}
