using PlatformShared.CrmMessages.Org;
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

    // Org
    [JsonSerializable(typeof(CreateAssetMessage))]
    [JsonSerializable(typeof(ReadAssetSensitiveDataMessage))]
    [JsonSerializable(typeof(UpdateAssetMessage))]

    [JsonSerializable(typeof(CreateDeptMessage))]
    [JsonSerializable(typeof(UpdateDeptMessage))]

    [JsonSerializable(typeof(UpdateCultureMessage))]
    [JsonSerializable(typeof(UpdateSettingsMessage))]

    [JsonSerializable(typeof(UpdateUserMessage))]

    // Person
    [JsonSerializable(typeof(CreatePersonAddressMessage))]
    [JsonSerializable(typeof(CreatePersonLocationMessage))]
    [JsonSerializable(typeof(DeletePersonAddressMessage))]
    [JsonSerializable(typeof(UpdatePersonAddressMessage))]

    [JsonSerializable(typeof(CreatePersonCategoryMessage))]
    [JsonSerializable(typeof(MergePersonCategoryMessage))]
    [JsonSerializable(typeof(SortPersonCategoryMessage))]
    [JsonSerializable(typeof(UpdatePersonCategoryMessage))]

    [JsonSerializable(typeof(CreatePersonInfoMessage))]
    [JsonSerializable(typeof(DeletePersonInfoMessage))]
    [JsonSerializable(typeof(UpdatePersonInfoMessage))]

    [JsonSerializable(typeof(AddContactRelationMessage))]
    [JsonSerializable(typeof(CreateContactMessage))]
    [JsonSerializable(typeof(DeleteContactRelationMessage))]
    [JsonSerializable(typeof(UpdateContactRelationMessage))]

    [JsonSerializable(typeof(CreateCustomerMessage))]
    [JsonSerializable(typeof(CreateSupplierMessage))]
    [JsonSerializable(typeof(DeletePersonMessage))]
    [JsonSerializable(typeof(ReadPersonMessage))]
    [JsonSerializable(typeof(UpdateCustomerMessage))]
    [JsonSerializable(typeof(UpdatePersonMessage))]
    [JsonSerializable(typeof(UpdateSupplierMessage))]

    [JsonSerializable(typeof(CreatePersonProductMessage))]
    [JsonSerializable(typeof(DeletePersonProductMessage))]
    [JsonSerializable(typeof(UpdatePersonProductMessage))]

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
