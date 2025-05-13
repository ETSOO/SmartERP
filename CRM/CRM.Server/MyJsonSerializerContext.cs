using CRM.Server.Dto.Person;
using CRM.Server.Dto.PersonProfile;
using CRM.Server.Dto.System;
using CRM.Server.RQ.Person;
using CRM.Server.RQ.PersonProfile;
using CRM.Server.RQ.System;
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

    // System
    [JsonSerializable(typeof(IEnumerable<PermissionItem>))]
    [JsonSerializable(typeof(SystemSettings))]

    [JsonSerializable(typeof(UpdateSettingsRQ))]

    // Results.ValidationProblem
    [JsonSerializable(typeof(HttpValidationProblemDetails))]
    [JsonSerializable(typeof(IFormFile))]
    [JsonSerializable(typeof(ProblemDetails))]

    public partial class MyJsonSerializerContext : JsonSerializerContext
    {
    }
}
