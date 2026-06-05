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
    [JsonSerializable(typeof(CreatePersonAddressMessage))]
    [JsonSerializable(typeof(CreatePersonLocationMessage))]
    [JsonSerializable(typeof(DeletePersonAddressMessage))]
    [JsonSerializable(typeof(UpdatePersonAddressMessage))]

    [JsonSerializable(typeof(CreateCustomerMessage))]
    [JsonSerializable(typeof(CreateSupplierMessage))]
    [JsonSerializable(typeof(DeletePersonMessage))]
    [JsonSerializable(typeof(ReadPersonMessage))]
    [JsonSerializable(typeof(UpdateCustomerMessage))]
    [JsonSerializable(typeof(UpdatePersonMessage))]
    [JsonSerializable(typeof(UpdateSupplierMessage))]

    [JsonSerializable(typeof(CreatePersonProfileLinkMessage))]
    [JsonSerializable(typeof(CreatePersonProfileMessage))]
    [JsonSerializable(typeof(DeletePersonProfileAttachmentMessage))]
    [JsonSerializable(typeof(DeletePersonProfileLinkMessage))]
    [JsonSerializable(typeof(ReadPersonProfileMessage))]
    [JsonSerializable(typeof(UpdatePersonProfileLinkMessage))]
    [JsonSerializable(typeof(UpdatePersonProfileMessage))]

    public partial class CrmJsonSerializerContext : JsonSerializerContext
    {
    }
}
