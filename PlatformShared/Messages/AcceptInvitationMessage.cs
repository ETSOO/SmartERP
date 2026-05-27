using com.etsoo.Utils.Serialization;
using PlatformShared.Dto;

namespace PlatformShared.Messages
{
    /// <summary>
    /// Accept invitation message
    /// 接受邀请信息
    /// </summary>
    public record AcceptInvitationMessage : CommonMessage, IMessageQueueMessage
    {
        public static string Type => "AcceptInvitation";

        /// <summary>
        /// Inviter data
        /// 邀请人信息
        /// </summary>
        public required CodeUserData UserData { get; init; }

        public override Dictionary<string, object?>? GetJsonData() => new()
        {
            ["Inviter"] = UserData.Name,
            ["Invitee"] = Data.UserName,
            ["OrgId"] = UserData.OrganizationId,
            ["OrgName"] = UserData.OrganizationName
        };
    }
}
