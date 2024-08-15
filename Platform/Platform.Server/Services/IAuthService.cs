using com.etsoo.ApiModel.Auth;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using Platform.Server.Database.Models;
using Platform.Server.Dto.Auth;

namespace Platform.Server.Services
{
    public interface IAuthService : ICommonService
    {
        ValueTask<IActionResult> CompleteRegisterAsync(CompleteRegisterData data, CancellationToken cancellationToken = default);
        IResult GetLogInUrl(IAuthClient client, string? userAgent, string deviceId);
        IResult GetSignUpUrl(IAuthClient client, string? userAgent, string deviceId);
        ValueTask LogInAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> LoginWithPwdAsync(LoginData data, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult, LoginUserWithPassword?)> LoginIdAsync(string id, string region, CancellationToken cancellationToken = default);
        ValueTask SignUpAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> WebInitCallAsync(InitCallRQ rq, string identifier);
        ValueTask<IActionResult> SendEmailAsync(SendEmailData data, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> SendSMSAsync(SendSMSData data, CancellationToken cancellationToken = default);
        Task<ActionResult> ValidateEmailRegistrationAsync(ValidateCodeData data, CancellationToken cancellationToken = default);
        Task<ActionResult> ValidateMobileRegistrationAsync(ValidateCodeData data, CancellationToken cancellationToken = default);
        ValueTask<RegisterUserData?> ViewRegisterDataAsync(CancellationToken cancellationToken = default);
    }
}