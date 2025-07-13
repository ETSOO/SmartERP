using PlatformShared.Dto;
using PlatformShared.Messages;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlatformShared
{
    /// <summary>
    /// JSON serializer context
    /// JSON 序列化器上下文
    /// </summary>
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    )]

    [JsonSerializable(typeof(Services.ApiOptions.SMTPApiOptions))]
    [JsonSerializable(typeof(Services.ApiOptions.StorageApiOptions))]

    [JsonSerializable(typeof(AppUrl[]))]

    [JsonSerializable(typeof(CommonMessage))]
    [JsonSerializable(typeof(IEnumerable<ContactItem>))]

    [JsonSerializable(typeof(AcceptInvitationMessageData))]
    [JsonSerializable(typeof(AdjustReportToMessageData))]
    [JsonSerializable(typeof(AdminClearUserFrozenMessageData))]
    [JsonSerializable(typeof(AdminRenewAppMessageData))]
    [JsonSerializable(typeof(AdminSupportMessageData))]
    [JsonSerializable(typeof(AddUserIdentifierMessageData))]
    [JsonSerializable(typeof(BuyAppMessageData))]
    [JsonSerializable(typeof(CommonUpdateMessageData))]
    [JsonSerializable(typeof(DeleteMemberMessageData))]
    [JsonSerializable(typeof(LeaveOrgMessageData))]
    [JsonSerializable(typeof(LoginFailedMessageData))]
    [JsonSerializable(typeof(LoginSuccessMessageData))]
    [JsonSerializable(typeof(RenewAppMessageData))]
    [JsonSerializable(typeof(SwitchOrgMessageData))]

    [JsonSerializable(typeof(JsonElement))]

    public partial class PlatformSharedContext : JsonSerializerContext
    {
    }
}
