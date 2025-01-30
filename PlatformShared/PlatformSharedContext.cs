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

    [JsonSerializable(typeof(CommonMessage))]
    [JsonSerializable(typeof(LoginFailedMessageData))]
    [JsonSerializable(typeof(SendEmailMessage))]
    [JsonSerializable(typeof(SendSMSMessage))]

    public partial class PlatformSharedContext : JsonSerializerContext
    {
    }
}
