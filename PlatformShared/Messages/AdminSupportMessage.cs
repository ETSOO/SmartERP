using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    public record AdminSupportMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "AdminSupport";

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
        /// Requester name
        /// 请求人姓名
        /// </summary>
        public required string RequesterName { get; init; }

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
        /// Approver name
        /// 批准人姓名
        /// </summary>
        public required string ApproverName { get; init; }

        /// <summary>
        /// Comment
        /// 备注
        /// </summary>
        public required string Comment { get; init; }

        /// <summary>
        /// Owner id
        /// 所有人编号
        /// </summary>
        public required int OwnerId { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(Comment)] = Comment,
            [nameof(ApproverLocalId)] = ApproverLocalId,
            [nameof(ApproverName)] = ApproverName,
            [nameof(Requester)] = Requester,
            [nameof(RequesterLocalId)] = RequesterLocalId,
            [nameof(RequesterName)] = RequesterName
        };
    }
}
