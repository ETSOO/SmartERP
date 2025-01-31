using PlatformShared.Messages;
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

    [JsonSerializable(typeof(AcceptInvitationMessage))]
    [JsonSerializable(typeof(CommonMessage))]
    [JsonSerializable(typeof(ResetPasswordMessage))]
    [JsonSerializable(typeof(SendEmailMessage))]
    [JsonSerializable(typeof(SendSMSMessage))]

    [JsonSerializable(typeof(AcceptInvitationMessageData))]
    [JsonSerializable(typeof(LoginFailedMessageData))]
    [JsonSerializable(typeof(LoginSuccessMessageData))]

    public partial class PlatformSharedContext : JsonSerializerContext
    {
    }
}
