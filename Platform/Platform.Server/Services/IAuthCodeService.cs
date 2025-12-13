using com.etsoo.Utils.Actions;
using Platform.Server.Dto.AuthCode;
using Platform.Server.Endpoints.AuthCode.RQ;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using System.Text.Json.Serialization.Metadata;

namespace Platform.Server.Services
{
    public interface IAuthCodeService : ICommonService
    {
        (IActionResult result, ValidateCodeData? data) CreateValidateCodeData(CodeValidateRQ rq, string? userAgent);
        Task<ValidateResultData?> ReadAsync(Guid id, AuthCodeAction action, CancellationToken cancellationToken = default);

        ValueTask<IActionResult> SendEmailAsync(EmailCodeRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> SendEmailAsync(SendEmailData data, Func<AuthCodeEmailTemplateView, (AuthCodeEmailTemplateView, string?)>? enhancer = null, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> SendEmailAsync<D>(SendEmailData<D> data, JsonTypeInfo<D> typeInfo, CancellationToken cancellationToken = default) where D : AuthCodeData;

        ValueTask<IActionResult> SendSMSAsync(SMSCodeRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> SendSMSAsync<D>(SendSMSData<D> data, JsonTypeInfo<D> typeInfo, CancellationToken cancellationToken = default) where D : AuthCodeData;
        ValueTask<IActionResult> SendSMSAsync(SendSMSData data, Action<CoreAuthCode>? enhancer = null, CancellationToken cancellationToken = default);

        Task<(ActionResult result, ValidateResultData? data)> ValidateAsync(AuthCodeAction actionId, ValidateCodeData data, CancellationToken cancellationToken = default);
    }
}