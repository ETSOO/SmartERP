using com.etsoo.Utils.Serialization;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Delete member message
    /// 删除成员消息
    /// </summary>
    public record DeleteMemberMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "DeleteMember";

        /// <summary>
        /// Organization name
        /// 机构名称
        /// </summary>
        public required string OrgName { get; init; }

        /// <summary>
        /// Inviter id
        /// 邀请人编号
        /// </summary>
        public int? InviterId { get; init; }

        /// <summary>
        /// Inviter name
        /// 邀请人姓名
        /// </summary>
        public string? InviterName { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            [nameof(InviterName)] = InviterName
        };
    }
}
