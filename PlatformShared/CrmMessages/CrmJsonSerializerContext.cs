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

    [JsonSerializable(typeof(UpdatePersonProfileMessage))]
    public partial class CrmJsonSerializerContext : JsonSerializerContext
    {
    }
}
