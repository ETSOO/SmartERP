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

    [JsonSerializable(typeof(AuthCodeActionItem))]
    [JsonSerializable(typeof(AuthCodeData))]
    [JsonSerializable(typeof(CommonUpdateMessageData))]
    [JsonSerializable(typeof(CustomResourceData[]))]
    [JsonSerializable(typeof(PersonProductJsonData))]
    [JsonSerializable(typeof(SendAuthCodeEmailMessage))]

    [JsonSerializable(typeof(JsonElement))]

    public partial class PlatformSharedContext : JsonSerializerContext
    {
    }
}
