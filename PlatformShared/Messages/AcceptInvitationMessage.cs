using com.etsoo.MessageQueue;
using PlatformShared.Dto;
using System.Text.Json;

namespace PlatformShared.Messages
{
    public record AcceptInvitationMessageData
    {
        public required string Inviter { get; init; }
        public required string Invitee { get; init; }
        public required int OrgId { get; init; }
        public required string OrgName { get; init; }
    }

    /// <summary>
    /// Accept invitation message
    /// 接受邀请信息
    /// </summary>
    public record AcceptInvitationMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "AcceptInvitation";

        /// <summary>
        /// Inviter id
        /// 邀请人编号
        /// </summary>
        public required int InviterId { get; init; }

        /// <summary>
        /// Inviter data
        /// 邀请人信息
        /// </summary>
        public required CodeUserData UserData { get; init; }

        public override string? GetMoreData()
        {
            return JsonSerializer.Serialize(new AcceptInvitationMessageData
            {
                Inviter = UserData.Name,
                Invitee = Data.UserName,
                OrgId = UserData.OrganizationId,
                OrgName = UserData.OrganizationName
            }, PlatformSharedContext.Default.AcceptInvitationMessageData);
        }
    }
}
