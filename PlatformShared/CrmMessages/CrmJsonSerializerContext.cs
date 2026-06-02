using PlatformShared.CrmMessages.Person;
using System.Text.Json.Serialization;

namespace PlatformShared.CrmMessages
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

    // Person
    [JsonSerializable(typeof(CreateCustomerMessage))]
    [JsonSerializable(typeof(CreatePersonProfileLinkMessage))]
    [JsonSerializable(typeof(CreatePersonProfileMessage))]
    [JsonSerializable(typeof(DeletePersonProfileAttachmentMessage))]
    [JsonSerializable(typeof(DeletePersonProfileLinkMessage))]
    [JsonSerializable(typeof(ReadPersonProfileMessage))]
    [JsonSerializable(typeof(UpdateCustomerMessage))]
    [JsonSerializable(typeof(UpdatePersonProfileLinkMessage))]
    [JsonSerializable(typeof(UpdatePersonProfileMessage))]
    public partial class CrmJsonSerializerContext : JsonSerializerContext
    {
    }
}
