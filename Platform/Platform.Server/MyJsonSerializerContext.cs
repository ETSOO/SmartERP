using System.Text.Json.Serialization;

namespace Platform.Server
{
    /// <summary>
    /// JSON serializer context
    /// JSON 序列化器上下文
    /// </summary>
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    )]
    [JsonSerializable(typeof(bool))]
    public partial class MyJsonSerializerContext : JsonSerializerContext
    {
    }
}
