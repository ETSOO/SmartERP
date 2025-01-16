using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Localization.Country;
using com.etsoo.SMS;
using Microsoft.AspNetCore.Mvc;
using Platform.Server.Dto.App;
using Platform.Server.Dto.Auth;
using Platform.Server.Dto.Org;
using Platform.Server.Dto.Public;
using Platform.Server.Endpoints.App.RQ;
using Platform.Server.Endpoints.Auth.RQ;
using Platform.Server.Endpoints.Member.RQ;
using Platform.Server.Endpoints.Org.RQ;
using Platform.Server.Endpoints.Public.RQ;
using Platform.Server.Endpoints.User.RQ;
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
    [JsonSerializable(typeof(AppBuyNewRQ))]
    [JsonSerializable(typeof(AppBuyRQ))]
    [JsonSerializable(typeof(AppCreateApiKeyRQ))]
    [JsonSerializable(typeof(AppListRQ))]
    [JsonSerializable(typeof(AppGetMyRQ))]
    [JsonSerializable(typeof(AppPurchasedQueryRQ))]
    [JsonSerializable(typeof(AppQueryRQ))]
    [JsonSerializable(typeof(AppRenewRQ))]
    [JsonSerializable(typeof(AppUpdateRQ))]

    [JsonSerializable(typeof(IEnumerable<AppQueryData>))]

    // Auth
    [JsonSerializable(typeof(CodeValidateRQ))]
    [JsonSerializable(typeof(CompleteRegisterRQ))]
    [JsonSerializable(typeof(EmailCodeRQ))]
    [JsonSerializable(typeof(ResetPasswordRQ))]
    [JsonSerializable(typeof(SMSCodeRQ))]

    [JsonSerializable(typeof(AuthCodeAction))]
    [JsonSerializable(typeof(RegisterUserData))]
    [JsonSerializable(typeof(SendEmailData))]
    [JsonSerializable(typeof(SendSMSData))]

    // Members
    [JsonSerializable(typeof(IEnumerable<MemberListRQ>))]
    [JsonSerializable(typeof(MemberQueryRQ))]

    // Org
    [JsonSerializable(typeof(OrgCreateRQ))]
    [JsonSerializable(typeof(OrgListRQ))]
    [JsonSerializable(typeof(OrgGetMyRQ))]
    [JsonSerializable(typeof(OrgUpdateRQ))]
    [JsonSerializable(typeof(OrgQueryRQ))]

    [JsonSerializable(typeof(IEnumerable<OrgGetMyData>))]
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

    // User
    [JsonSerializable(typeof(AuditHistoryRQ))]

    // Others
    [JsonSerializable(typeof(TemplateItem))]

    // Results.ValidationProblem
    [JsonSerializable(typeof(HttpValidationProblemDetails))]
    [JsonSerializable(typeof(IFormFile))]
    [JsonSerializable(typeof(ProblemDetails))]

    public partial class MyJsonSerializerContext : JsonSerializerContext
    {
    }
}
