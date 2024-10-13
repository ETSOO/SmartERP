using com.etsoo.SMS;
using Platform.Server.Dto.App;
using Platform.Server.Dto.Auth;
using Platform.Server.Dto.Public;
using Platform.Server.Endpoints.Auth.RQ;
using Platform.Server.Endpoints.Public.RQ;
using System.Text.Json.Serialization;

namespace Platform.Server
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

    // App
    [JsonSerializable(typeof(IEnumerable<AppData>))]

    // Auth
    [JsonSerializable(typeof(CodeValidateRQ))]
    [JsonSerializable(typeof(CompleteRegisterRQ))]
    [JsonSerializable(typeof(EmailCodeRQ))]
    [JsonSerializable(typeof(SMSCodeRQ))]
    [JsonSerializable(typeof(SwitchOrgRQ))]

    [JsonSerializable(typeof(AuthCodeAction))]
    [JsonSerializable(typeof(RegisterUserData))]
    [JsonSerializable(typeof(SendEmailData))]
    [JsonSerializable(typeof(SendSMSData))]

    // Public
    [JsonSerializable(typeof(MobileQRCodeRQ))]
    [JsonSerializable(typeof(OrgInfoRQ))]

    [JsonSerializable(typeof(OrgPublicInfo))]

    // Others
    [JsonSerializable(typeof(TemplateItem))]
    public partial class MyJsonSerializerContext : JsonSerializerContext
    {
    }
}
