using PlatformShared.Dto;
using PlatformShared.Dto.Document.Order;
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

    [JsonSerializable(typeof(OrderLineData))]

    [JsonSerializable(typeof(JsonElement))]
    [JsonSerializable(typeof(IEnumerable<long>))]
    [JsonSerializable(typeof(IEnumerable<ulong>))]
    [JsonSerializable(typeof(IEnumerable<int>))]
    [JsonSerializable(typeof(IEnumerable<uint>))]
    [JsonSerializable(typeof(IEnumerable<short>))]
    [JsonSerializable(typeof(IEnumerable<ushort>))]
    [JsonSerializable(typeof(IEnumerable<byte>))]
    [JsonSerializable(typeof(IEnumerable<Guid>))]
    [JsonSerializable(typeof(IEnumerable<string>))]
    [JsonSerializable(typeof(IEnumerable<double>))]
    [JsonSerializable(typeof(IEnumerable<decimal>))]
    [JsonSerializable(typeof(IEnumerable<float>))]
    [JsonSerializable(typeof(IEnumerable<DateOnly>))]
    [JsonSerializable(typeof(IEnumerable<TimeOnly>))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(DateTime))]
    [JsonSerializable(typeof(DateTimeOffset))]

    public partial class PlatformSharedContext : JsonSerializerContext
    {
    }
}
