using CRM.Server.Dto.Asset;
using CRM.Server.Dto.Customer;
using CRM.Server.Dto.Dept;
using CRM.Server.Dto.Group;
using CRM.Server.Dto.Order;
using CRM.Server.Dto.Person;
using CRM.Server.Dto.PersonProfile;
using CRM.Server.Dto.PO;
using CRM.Server.Dto.Product;
using CRM.Server.Dto.Supplier;
using CRM.Server.Dto.System;
using CRM.Server.Dto.User;
using CRM.Server.RQ.Asset;
using CRM.Server.RQ.Customer;
using CRM.Server.RQ.Dept;
using CRM.Server.RQ.Group;
using CRM.Server.RQ.Order;
using CRM.Server.RQ.Person;
using CRM.Server.RQ.PersonProfile;
using CRM.Server.RQ.PO;
using CRM.Server.RQ.Product;
using CRM.Server.RQ.Supplier;
using CRM.Server.RQ.System;
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
    [JsonSerializable(typeof(AssetListData))]
    [JsonSerializable(typeof(AssetQueryData[]))]

    [JsonSerializable(typeof(AssetListRQ))]

    // Customer
    [JsonSerializable(typeof(CustomerListData))]
    [JsonSerializable(typeof(CustomerQueryData[]))]

    [JsonSerializable(typeof(CustomerListRQ))]

    // Dept
    [JsonSerializable(typeof(DeptListData))]
    [JsonSerializable(typeof(DeptQueryData[]))]

    [JsonSerializable(typeof(DeptCreateRQ))]
    [JsonSerializable(typeof(DeptListRQ))]
    [JsonSerializable(typeof(DeptUpdateRQ))]

    // Group
    [JsonSerializable(typeof(GroupListData))]
    [JsonSerializable(typeof(GroupQueryData[]))]
    [JsonSerializable(typeof(GroupViewData))]

    [JsonSerializable(typeof(GroupListRQ))]

    // Order
    [JsonSerializable(typeof(OrderListData))]
    [JsonSerializable(typeof(OrderQueryData[]))]

    [JsonSerializable(typeof(OrderListRQ))]

    // Person
    [JsonSerializable(typeof(ChoosePersonsData))]
    [JsonSerializable(typeof(IEnumerable<PersonQueryData>))]
    [JsonSerializable(typeof(PersonViewData))]

    [JsonSerializable(typeof(ChoosePersonsRQ))]
    [JsonSerializable(typeof(PersonListRQ))]

    // Person profile
    [JsonSerializable(typeof(PersonProfileInnerViewData))]
    [JsonSerializable(typeof(PersonProfileListData))]
    [JsonSerializable(typeof(PersonProfileQueryData))]
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
    [JsonSerializable(typeof(POListData))]
    [JsonSerializable(typeof(POQueryData[]))]

    [JsonSerializable(typeof(POListRQ))]

    // Product
    [JsonSerializable(typeof(ProductListData))]
    [JsonSerializable(typeof(ProductQueryData[]))]

    [JsonSerializable(typeof(ProductListRQ))]

    // Supplier
    [JsonSerializable(typeof(SupplierListData))]
    [JsonSerializable(typeof(SupplierQueryData[]))]

    [JsonSerializable(typeof(SupplierListRQ))]

    // System
    [JsonSerializable(typeof(IEnumerable<PermissionItem>))]
    [JsonSerializable(typeof(SystemSettings))]

    [JsonSerializable(typeof(UpdateSettingsRQ))]

    // User
    [JsonSerializable(typeof(UserListData))]
    [JsonSerializable(typeof(UserQueryData[]))]
    [JsonSerializable(typeof(UserUpdateReadData))]

    [JsonSerializable(typeof(UserListRQ))]
    [JsonSerializable(typeof(UserUpdateRQ))]

    // Results.ValidationProblem
    [JsonSerializable(typeof(HttpValidationProblemDetails))]
    [JsonSerializable(typeof(IFormFile))]
    [JsonSerializable(typeof(ProblemDetails))]

    public partial class MyJsonSerializerContext : JsonSerializerContext
    {
    }
}
