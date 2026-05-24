using com.etsoo.ApiModel.Dto.Maps;
using com.etsoo.ApiModel.RQ.Maps;
using com.etsoo.ImageUtils.Barcode;
using com.etsoo.Localization;
using com.etsoo.SMS;
using Microsoft.AspNetCore.Mvc;
using Platform.Server.Dto.App;
using Platform.Server.Dto.Auth;
using Platform.Server.Dto.AuthCode;
using Platform.Server.Dto.Member;
using Platform.Server.Dto.Org;
using Platform.Server.Dto.Public;
using Platform.Server.Endpoints.App.RQ;
using Platform.Server.Endpoints.Auth.RQ;
using Platform.Server.Endpoints.AuthCode.RQ;
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
    [JsonSerializable(typeof(AppRenewRQ))]
    [JsonSerializable(typeof(AppUpdateRQ))]

    [JsonSerializable(typeof(IEnumerable<AppQueryData>))]

    // Auth
    [JsonSerializable(typeof(AdminSupportRQ))]
    [JsonSerializable(typeof(CheckUserIdentifierRQ))]
    [JsonSerializable(typeof(CompleteRegisterRQ))]
    [JsonSerializable(typeof(ResetPasswordRQ))]

    [JsonSerializable(typeof(CheckUserIdentifierData))]
    [JsonSerializable(typeof(RegisterUserData))]

    // Auth Code
    [JsonSerializable(typeof(CodeValidateRQ))]
    [JsonSerializable(typeof(EmailCodeRQ))]
    [JsonSerializable(typeof(SMSCodeRQ))]

    [JsonSerializable(typeof(SendEmailData))]
    [JsonSerializable(typeof(SendSMSData))]

    // Document
    [JsonSerializable(typeof(PlatformShared.Dto.SystemDocumentListData[]))]
    [JsonSerializable(typeof(PlatformShared.Dto.SystemDocumentViewData))]

    [JsonSerializable(typeof(PlatformShared.RQ.SystemDocumentListRQ))]

    // Members
    [JsonSerializable(typeof(MemberAdjustReportToRQ))]
    [JsonSerializable(typeof(IEnumerable<MemberListRQ>))]
    [JsonSerializable(typeof(MemberInviteRQ))]
    [JsonSerializable(typeof(MemberUpdateRQ))]

    [JsonSerializable(typeof(MemberInvitationData))]
    [JsonSerializable(typeof(IEnumerable<MemberQueryData>))]

    // Org
    [JsonSerializable(typeof(OrgCreateApiRQ))]
    [JsonSerializable(typeof(OrgCreateResourceRQ))]
    [JsonSerializable(typeof(OrgCreateRQ))]
    [JsonSerializable(typeof(OrgListRQ))]
    [JsonSerializable(typeof(OrgQueryApiRQ))]
    [JsonSerializable(typeof(OrgQueryResourceRQ))]
    [JsonSerializable(typeof(OrgGetMyRQ))]
    [JsonSerializable(typeof(OrgUpdateRQ))]
    [JsonSerializable(typeof(OrgUpdateApiRQ))]
    [JsonSerializable(typeof(SendProfileEmailRQ))]

    [JsonSerializable(typeof(IEnumerable<OrgGetMyData>))]
    [JsonSerializable(typeof(IEnumerable<OrgQueryData>))]
    [JsonSerializable(typeof(IEnumerable<OrgQueryResourceData>))]
    [JsonSerializable(typeof(OrgUpdateResourceReadData))]

    // Public
    [JsonSerializable(typeof(AcceptInvitationRQ))]
    [JsonSerializable(typeof(MobileQRCodeRQ))]
    [JsonSerializable(typeof(OrgInfoRQ))]
    [JsonSerializable(typeof(ParseNameRQ))]
    [JsonSerializable(typeof(PlaceQueryRQ))]

    [JsonSerializable(typeof(BarcodeOptions))]
    [JsonSerializable(typeof(NameData))]
    [JsonSerializable(typeof(OrgPublicInfo))]
    [JsonSerializable(typeof(IEnumerable<PlaceCommon>))]
    [JsonSerializable(typeof(IEnumerable<CustomResourceData>))]

    // User
    [JsonSerializable(typeof(AuditHistoryRQ))]
    [JsonSerializable(typeof(UserUpdateRQ))]

    // Others
    [JsonSerializable(typeof(TemplateItem))]

    // Results.ValidationProblem
    [JsonSerializable(typeof(HttpValidationProblemDetails))]
    [JsonSerializable(typeof(IFormFile))]
    [JsonSerializable(typeof(IFormFileCollection))]
    [JsonSerializable(typeof(ProblemDetails))]
    [JsonSerializable(typeof(IResult))]

    public partial class MyJsonSerializerContext : JsonSerializerContext
    {
    }
}
