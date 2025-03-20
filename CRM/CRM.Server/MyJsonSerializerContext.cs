using CRM.Server.Dto.Person;
using CRM.Server.RQ.Person;
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

    // Person
    [JsonSerializable(typeof(PersonQueryData))]
    [JsonSerializable(typeof(PersonViewData))]

    [JsonSerializable(typeof(PersonListRQ))]

    // Results.ValidationProblem
    [JsonSerializable(typeof(HttpValidationProblemDetails))]
    [JsonSerializable(typeof(IFormFile))]
    [JsonSerializable(typeof(ProblemDetails))]

    public partial class MyJsonSerializerContext : JsonSerializerContext
    {
    }
}
