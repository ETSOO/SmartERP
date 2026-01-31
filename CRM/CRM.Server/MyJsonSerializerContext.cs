using CRM.Server.Dto.Asset;
using CRM.Server.Dto.Customer;
using CRM.Server.Dto.Dept;
using CRM.Server.Dto.Group;
using CRM.Server.Dto.Order;
using CRM.Server.Dto.Person;
using CRM.Server.Dto.PersonAddress;
using CRM.Server.Dto.PersonCategory;
using CRM.Server.Dto.PersonContact;
using CRM.Server.Dto.PersonProfile;
using CRM.Server.Dto.PO;
using CRM.Server.Dto.Product;
using CRM.Server.Dto.ProductCategory;
using CRM.Server.Dto.Promotion;
using CRM.Server.Dto.Supplier;
using CRM.Server.Dto.System;
using CRM.Server.Dto.Tag;
using CRM.Server.Dto.User;
using CRM.Server.RQ.Asset;
using CRM.Server.RQ.Customer;
using CRM.Server.RQ.Dept;
using CRM.Server.RQ.Group;
using CRM.Server.RQ.Order;
using CRM.Server.RQ.Person;
using CRM.Server.RQ.PersonAddress;
using CRM.Server.RQ.PersonCategory;
using CRM.Server.RQ.PersonContact;
using CRM.Server.RQ.PersonInfo;
using CRM.Server.RQ.PersonProfile;
using CRM.Server.RQ.PO;
using CRM.Server.RQ.Product;
using CRM.Server.RQ.ProductCategory;
using CRM.Server.RQ.Promotion;
using CRM.Server.RQ.Supplier;
using CRM.Server.RQ.System;
using CRM.Server.RQ.Tag;
using CRM.Server.RQ.User;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace CRM.Server
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

    // Asset
    [JsonSerializable(typeof(AssetListData[]))]
    [JsonSerializable(typeof(AssetQueryData[]))]
    [JsonSerializable(typeof(AssetUpdateReadData))]

    [JsonSerializable(typeof(AssetCreateRQ))]
    [JsonSerializable(typeof(AssetListRQ))]
    [JsonSerializable(typeof(AssetUpdateRQ))]

    // Customer
    [JsonSerializable(typeof(CustomerListData[]))]
    [JsonSerializable(typeof(CustomerQueryData[]))]
    [JsonSerializable(typeof(CustomerUpdateReadData))]

    [JsonSerializable(typeof(CustomerCreateRQ))]
    [JsonSerializable(typeof(CustomerListRQ))]
    [JsonSerializable(typeof(CustomerUpdateRQ))]

    // Dept
    [JsonSerializable(typeof(DeptListData[]))]
    [JsonSerializable(typeof(DeptQueryData[]))]

    [JsonSerializable(typeof(DeptCreateRQ))]
    [JsonSerializable(typeof(DeptListRQ))]
    [JsonSerializable(typeof(DeptUpdateRQ))]

    // Group
    [JsonSerializable(typeof(GroupListData[]))]
    [JsonSerializable(typeof(GroupQueryData[]))]
    [JsonSerializable(typeof(GroupViewData))]

    [JsonSerializable(typeof(GroupListRQ))]

    // Order
    [JsonSerializable(typeof(OrderListData[]))]
    [JsonSerializable(typeof(OrderQueryData[]))]

    [JsonSerializable(typeof(OrderListRQ))]

    // Person
    [JsonSerializable(typeof(ChoosePersonsData))]
    [JsonSerializable(typeof(PersonDuplicateTestData[]))]
    [JsonSerializable(typeof(IEnumerable<PersonQueryData>))]
    [JsonSerializable(typeof(PersonUpdateReadData))]
    [JsonSerializable(typeof(PersonViewData))]

    [JsonSerializable(typeof(ChoosePersonsRQ))]
    [JsonSerializable(typeof(PersonDuplicateTestRQ))]
    [JsonSerializable(typeof(PersonListRQ))]
    [JsonSerializable(typeof(PersonUpdateRQ))]

    // Person address
    [JsonSerializable(typeof(AddressListData[]))]
    [JsonSerializable(typeof(AddressQueryData[]))]
    [JsonSerializable(typeof(AddressUpdateReadData))]

    [JsonSerializable(typeof(AddressCreateRQ))]
    [JsonSerializable(typeof(AddressListRQ))]
    [JsonSerializable(typeof(AddressLocationCreateRQ))]
    [JsonSerializable(typeof(AddressUpdateRQ))]

    // Person contact
    [JsonSerializable(typeof(IEnumerable<ContactQueryData>))]

    [JsonSerializable(typeof(ContactCreateRQ))]
    [JsonSerializable(typeof(ContactListRQ))]
    [JsonSerializable(typeof(ContactRelationAddRQ))]
    [JsonSerializable(typeof(ContactRelationUpdateRQ))]

    // Person category
    [JsonSerializable(typeof(PersonCategoryDuplicateTestData[]))]
    [JsonSerializable(typeof(PersonCategoryListData[]))]
    [JsonSerializable(typeof(PersonCategoryQueryData[]))]
    [JsonSerializable(typeof(PersonCategoryUpdateReadData))]

    [JsonSerializable(typeof(PersonCategoryCreateRQ))]
    [JsonSerializable(typeof(PersonCategoryDuplicateTestRQ))]
    [JsonSerializable(typeof(PersonCategoryListRQ))]
    [JsonSerializable(typeof(PersonCategoryUpdateRQ))]

    // Person info
    [JsonSerializable(typeof(PersonInfoCreateRQ))]
    [JsonSerializable(typeof(PersonInfoQueryRQ))]
    [JsonSerializable(typeof(PersonInfoUpdateRQ))]

    // Person profile
    [JsonSerializable(typeof(PersonProfileInnerViewData))]
    [JsonSerializable(typeof(PersonProfileListData[]))]
    [JsonSerializable(typeof(PersonProfileQueryData[]))]
    [JsonSerializable(typeof(PersonProfileViewData))]

    [JsonSerializable(typeof(PersonProfileAttachmentUpdateRQ))]
    [JsonSerializable(typeof(PersonProfileCreateRQ))]
    [JsonSerializable(typeof(PersonProfileLinkCreateRQ))]
    [JsonSerializable(typeof(PersonProfileLinkUpdateRQ))]
    [JsonSerializable(typeof(PersonProfileListRQ))]
    [JsonSerializable(typeof(PersonProfileQueryRQ))]
    [JsonSerializable(typeof(PersonProfileUpdateRQ))]
    [JsonSerializable(typeof(PersonTaskCreateRQ))]

    // PO
    [JsonSerializable(typeof(POListData[]))]
    [JsonSerializable(typeof(POQueryData[]))]

    [JsonSerializable(typeof(POListRQ))]

    // Product
    [JsonSerializable(typeof(ProductDuplicateTestData[]))]
    [JsonSerializable(typeof(ProductListData[]))]
    [JsonSerializable(typeof(ProductQueryData[]))]
    [JsonSerializable(typeof(ProductUnitItem[]))]
    [JsonSerializable(typeof(ProductUpdateReadData))]
    [JsonSerializable(typeof(ProductViewData))]

    [JsonSerializable(typeof(ProductCreateRQ))]
    [JsonSerializable(typeof(ProductDuplicateTestRQ))]
    [JsonSerializable(typeof(ProductListRQ))]
    [JsonSerializable(typeof(ProductUnitUpdateRQ))]
    [JsonSerializable(typeof(ProductUpdateLogoRQ))]
    [JsonSerializable(typeof(ProductUpdateRQ))]

    // Product category
    [JsonSerializable(typeof(ProductCategoryDuplicateTestData[]))]
    [JsonSerializable(typeof(ProductCategoryListData[]))]
    [JsonSerializable(typeof(ProductCategoryQueryData[]))]
    [JsonSerializable(typeof(ProductCategoryUpdateReadData))]
    [JsonSerializable(typeof(ProductCategoryCreateRQ))]
    [JsonSerializable(typeof(ProductCategoryDuplicateTestRQ))]
    [JsonSerializable(typeof(ProductCategoryListRQ))]
    [JsonSerializable(typeof(ProductCategoryUpdateRQ))]

    // Promotion
    [JsonSerializable(typeof(PromotionListData[]))]
    [JsonSerializable(typeof(PromotionQueryData[]))]
    [JsonSerializable(typeof(PromotionUpdateReadData))]
    [JsonSerializable(typeof(PromotionCreateRQ))]
    [JsonSerializable(typeof(PromotionListRQ))]
    [JsonSerializable(typeof(PromotionUpdateRQ))]

    // Supplier
    [JsonSerializable(typeof(SupplierListData[]))]
    [JsonSerializable(typeof(SupplierQueryData[]))]
    [JsonSerializable(typeof(SupplierUpdateReadData))]

    [JsonSerializable(typeof(SupplierCreateRQ))]
    [JsonSerializable(typeof(SupplierListRQ))]
    [JsonSerializable(typeof(SupplierUpdateRQ))]

    // System
    [JsonSerializable(typeof(CustomCultureItem))]
    [JsonSerializable(typeof(PermissionItem[]))]
    [JsonSerializable(typeof(SystemSettings))]

    [JsonSerializable(typeof(ReadCultureRQ))]
    [JsonSerializable(typeof(UpdateCultureRQ))]
    [JsonSerializable(typeof(UpdateSettingsRQ))]

    // Tag
    [JsonSerializable(typeof(TagQueryData[]))]

    [JsonSerializable(typeof(TagListRQ))]
    [JsonSerializable(typeof(TagQueryRQ))]

    // User
    [JsonSerializable(typeof(UserListData[]))]
    [JsonSerializable(typeof(UserQueryData[]))]
    [JsonSerializable(typeof(UserUpdateReadData))]

    [JsonSerializable(typeof(UserListRQ))]
    [JsonSerializable(typeof(UserUpdateRQ))]

    // Results.ValidationProblem
    [JsonSerializable(typeof(HttpValidationProblemDetails))]
    [JsonSerializable(typeof(IFormFile))]
    [JsonSerializable(typeof(ProblemDetails))]

    [JsonSerializable(typeof(com.etsoo.Utils.Actions.ActionResult))]

    public partial class MyJsonSerializerContext : JsonSerializerContext
    {
    }
}
