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

    [JsonSerializable(typeof(Dto.Query.ReadUserDto))]

    [JsonSerializable(typeof(RQ.Operation.AppRenewRQ))]

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
