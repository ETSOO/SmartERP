using PlatformShared.CrmMessages.Order;
using PlatformShared.CrmMessages.Org;
using PlatformShared.CrmMessages.Person;
using PlatformShared.CrmMessages.PO;
using PlatformShared.CrmMessages.Product;
using PlatformShared.CrmMessages.Stock;
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

    // Order
    [JsonSerializable(typeof(CreateOrderMessage))]
    [JsonSerializable(typeof(ReadOrderMessage))]
    [JsonSerializable(typeof(RecalculateOrderMessage))]
    [JsonSerializable(typeof(UpdateOrderMessage))]

    [JsonSerializable(typeof(CompleteOrderLineMessage))]
    [JsonSerializable(typeof(CreateOrderLineMessage))]
    [JsonSerializable(typeof(DeleteOrderLineMessage))]
    [JsonSerializable(typeof(ReadOrderLineMessage))]
    [JsonSerializable(typeof(RollbackOrderLineMessage))]
    [JsonSerializable(typeof(StartOrderLineMessage))]
    [JsonSerializable(typeof(UpdateOrderLineMessage))]

    [JsonSerializable(typeof(CreateOrderDeliveryMessage))]
    [JsonSerializable(typeof(SortOrderDeliveryMessage))]
    [JsonSerializable(typeof(UpdateOrderDeliveryMessage))]

    [JsonSerializable(typeof(CreateOrderPaymentMessage))]
    [JsonSerializable(typeof(SortOrderPaymentMessage))]
    [JsonSerializable(typeof(UpdateOrderPaymentMessage))]

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

    // Product
    [JsonSerializable(typeof(CreateProductMessage))]
    [JsonSerializable(typeof(DeleteProductMessage))]
    [JsonSerializable(typeof(ProductEditBomsMessage))]
    [JsonSerializable(typeof(UpdateProductMessage))]
    [JsonSerializable(typeof(UpdateProductLogoMessage))]
    [JsonSerializable(typeof(UpdateProductPriceMessage))]
    [JsonSerializable(typeof(UpdateProductUnitMessage))]

    [JsonSerializable(typeof(CreateProductCategoryMessage))]
    [JsonSerializable(typeof(MergeProductCategoryMessage))]
    [JsonSerializable(typeof(SortProductCategoryMessage))]
    [JsonSerializable(typeof(UpdateProductCategoryMessage))]

    [JsonSerializable(typeof(CreatePromotionMessage))]
    [JsonSerializable(typeof(SortPromotionMessage))]
    [JsonSerializable(typeof(UpdatePromotionMessage))]

    // PO
    [JsonSerializable(typeof(CreatePOMessage))]
    [JsonSerializable(typeof(ReadPOMessage))]
    [JsonSerializable(typeof(RecalculatePOMessage))]
    [JsonSerializable(typeof(UpdatePOMessage))]

    [JsonSerializable(typeof(CompletePOLineMessage))]
    [JsonSerializable(typeof(CreatePOLineMessage))]
    [JsonSerializable(typeof(DeletePOLineMessage))]
    [JsonSerializable(typeof(ReadPOLineMessage))]
    [JsonSerializable(typeof(RollbackPOLineMessage))]
    [JsonSerializable(typeof(StartPOLineMessage))]
    [JsonSerializable(typeof(UpdatePOLineMessage))]

    // Stock
    [JsonSerializable(typeof(StockAssembleMessage))]
    [JsonSerializable(typeof(StockCreateLineMessage))]
    [JsonSerializable(typeof(DeleteStockMessage))]
    [JsonSerializable(typeof(StockLoseMessage))]
    [JsonSerializable(typeof(StockInitMessage))]
    [JsonSerializable(typeof(StockOrderOutMessage))]
    [JsonSerializable(typeof(StockPOInMessage))]
    [JsonSerializable(typeof(ReadStockMessage))]
    [JsonSerializable(typeof(StockReceiveMessage))]
    [JsonSerializable(typeof(StockTakeMessage))]
    [JsonSerializable(typeof(StockTransferMessage))]
    [JsonSerializable(typeof(UpdateStockMessage))]
    [JsonSerializable(typeof(UpdateStockLineMessage))]

    public partial class CrmJsonSerializerContext : JsonSerializerContext
    {
    }
}
