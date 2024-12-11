using com.etsoo.ApiModel.Auth;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using Platform.Server.Dto.Auth;
using Platform.Server.Endpoints.Auth.RQ;
using PlatformShared.Database.Models;

namespace Platform.Server.Services
{
    public interface IAuthService : ICommonService
    {
        ValueTask<ApiTokenData?> ApiRefreshTokenAsync(ApiRefreshTokenRQ rq, CancellationToken cancellationToken = default);
        ValueTask<string> AuthRequestAsync(AuthRequest rq, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> ChangePasswordAsync(ChangePasswordRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> ChangePasswordAsync(ChangePasswordDto data, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult result, string? refreshToken)> CompleteRegisterAsync(CompleteRegisterRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        IResult GetLogInUrl(IAuthClient client, string? userAgent, string deviceId);
        IResult GetSignUpUrl(IAuthClient client, string? userAgent, string deviceId);
        ValueTask LogInAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult result, string? refreshToken)> LoginWithPwdAsync(LoginRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> LoginIdAsync(LoginIdRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult, LoginUserWithPassword?)> LoginIdAsync(string id, string region, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult result, string? newRefreshToken)> RefreshTokenAsync(RefreshTokenData data, CancellationToken cancellationToken = default);
        ValueTask SignUpAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> WebInitCallAsync(InitCallRQ rq, string identifier);
        ValueTask<IActionResult> SendEmailAsync(SendEmailData data, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> SendSMSAsync(SendSMSData data, CancellationToken cancellationToken = default);
        Task<ActionResult> ValidateEmailCallbackAsync(ValidateCodeData data, CancellationToken cancellationToken = default);
        Task<ActionResult> ValidateEmailRegistrationAsync(ValidateCodeData data, CancellationToken cancellationToken = default);
        Task<ActionResult> ValidateMobileCallbackAsync(ValidateCodeData data, CancellationToken cancellationToken = default);
        Task<ActionResult> ValidateMobileRegistrationAsync(ValidateCodeData data, CancellationToken cancellationToken = default);
        ValueTask<RegisterUserData?> ViewRegisterDataAsync(CancellationToken cancellationToken = default);
        ValueTask<AppTokenData?> OAuthCreateTokenAsync(AuthCreateTokenRQ rq, CancellationToken cancellationToken = default);
        ValueTask<AppTokenData?> OAuthRefreshTokenAsync(AuthRefreshTokenRQ rq, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult result, string? newRefreshToken)> OAuthRefreshTokenResultAsync(AuthRefreshTokenRQ rq, CancellationToken cancellationToken = default);
        Task OAuthUserInfoAsync(HttpResponse? response, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult result, string? refreshToken)> SwitchOrgAsync(SwitchOrgRQ rq, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> ResetPasswordAsync(ResetPasswordRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> SignoutAsync(string token);
    }
}