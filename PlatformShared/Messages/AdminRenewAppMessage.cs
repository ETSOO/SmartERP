using com.etsoo.MessageQueue;
using System.Text.Json;

namespace PlatformShared.Messages
{
    public record AdminRenewAppMessageData
    {
        public required int Months { get; init; }
        public required string Comment { get; init; }
        public required int ApproverLocalId { get; init; }
        public required int Requester { get; init; }
        public required int RequesterLocalId { get; init; }
        public required string RequesterOrgName { get; init; }
    }

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
        public required int RequesterLocalId { get; init; }

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
        public required int ApproverLocalId { get; init; }

        /// <summary>
        /// Comment
        /// 备注
        /// </summary>
        public required string Comment { get; init; }

        public override string? GetMoreData()
        {
            return JsonSerializer.Serialize(new AdminRenewAppMessageData
            {
                Months = Months,
                Comment = Comment,
                ApproverLocalId = ApproverLocalId,
                Requester = Requester,
                RequesterLocalId = RequesterLocalId,
                RequesterOrgName = RequesterOrgName
            }, PlatformSharedContext.Default.AdminRenewAppMessageData);
        }
    }
}
