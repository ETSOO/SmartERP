using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Localization.Country;
using com.etsoo.SMS;
using Platform.Server.Dto.App;
using Platform.Server.Dto.Auth;
using Platform.Server.Dto.Org;
using Platform.Server.Dto.Public;
using Platform.Server.Endpoints.Auth.RQ;
using Platform.Server.Endpoints.Org.RQ;
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

    // Org
    [JsonSerializable(typeof(OrgCreateRQ))]
    [JsonSerializable(typeof(OrgUpdateRQ))]
    [JsonSerializable(typeof(OrgQueryRQ))]

    [JsonSerializable(typeof(IEnumerable<OrgQueryData>))]

    // Public
    [JsonSerializable(typeof(MobileQRCodeRQ))]
    [JsonSerializable(typeof(OrgInfoRQ))]
    [JsonSerializable(typeof(PlaceQueryRQ))]

    [JsonSerializable(typeof(BarcodeOptions))]
    [JsonSerializable(typeof(OrgPublicInfo))]
    [JsonSerializable(typeof(IEnumerable<CurrencyItem>))]
    [JsonSerializable(typeof(IEnumerable<RegionData>))]
    [JsonSerializable(typeof(IEnumerable<PlaceCommon>))]

    // Others
    [JsonSerializable(typeof(TemplateItem))]

    // Results.ValidationProblem
    [JsonSerializable(typeof(HttpValidationProblemDetails))]

    public partial class MyJsonSerializerContext : JsonSerializerContext
    {
    }
}
