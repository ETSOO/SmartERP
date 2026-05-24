using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Admin.Server
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

    // Admin
    [JsonSerializable(typeof(RQ.Admin.AppRenewRQ))]

    // Document
    [JsonSerializable(typeof(Dto.Document.DocumentQueryData[]))]
    [JsonSerializable(typeof(Dto.Document.DocumentViewData))]

    [JsonSerializable(typeof(RQ.Document.DocumentCreateRQ))]
    [JsonSerializable(typeof(RQ.Document.DocumentQueryRQ))]
    [JsonSerializable(typeof(RQ.Document.DocumentUpdateRQ))]

    // Query
    [JsonSerializable(typeof(Dto.Query.ReadUserDto))]

    [JsonSerializable(typeof(RQ.Query.AuditHistoryRQ))]
    [JsonSerializable(typeof(RQ.Query.AllAppRQ))]
    [JsonSerializable(typeof(RQ.Query.AllOrgRQ))]
    [JsonSerializable(typeof(RQ.Query.AllUserRQ))]
    [JsonSerializable(typeof(RQ.Query.AppListRQ))]
    [JsonSerializable(typeof(RQ.Query.OrgListRQ))]
    [JsonSerializable(typeof(RQ.Query.UserListRQ))]

    // Results.ValidationProblem
    [JsonSerializable(typeof(HttpValidationProblemDetails))]
    [JsonSerializable(typeof(ProblemDetails))]

    public partial class MyJsonSerializerContext : JsonSerializerContext
    {
    }
}
