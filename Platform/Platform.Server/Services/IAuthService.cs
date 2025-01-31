using com.etsoo.ApiModel.Auth;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using Platform.Server.Dto.Auth;
using Platform.Server.Endpoints.Auth.RQ;
using Platform.Server.Endpoints.AuthCode.RQ;
using PlatformShared.Database.Models;

namespace Platform.Server.Services
{
    public interface IAuthService : ICommonService
    {
        ValueTask<ApiTokenData?> ApiRefreshTokenAsync(ApiRefreshTokenRQ rq, CancellationToken cancellationToken = default);
        ValueTask<string> AuthRequestAsync(AuthRequest rq, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> ChangePasswordAsync(ChangePasswordRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> ChangePasswordAsync(ChangePasswordDto data, CancellationToken cancellationToken = default);
        ValueTask<TristateEnum> CheckUserIdentifierAsync(CheckUserIdentifierRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<TristateEnum> CheckUserIdentifierAsync(CheckUserIdentifierData data, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult result, string? refreshToken)> CompleteRegisterAsync(CompleteRegisterRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        IResult GetLogInUrl(IAuthClient client, string? userAgent, string deviceId);
        IResult GetSignUpUrl(IAuthClient client, string? userAgent, string deviceId);
        ValueTask LogInAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult result, string? refreshToken)> LoginWithPwdAsync(LoginRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> LoginIdAsync(LoginIdRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult, LoginUserWithPassword?)> LoginIdAsync(string id, string region, string timezone, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult result, string? newRefreshToken)> RefreshTokenAsync(RefreshTokenData data, CancellationToken cancellationToken = default);
        ValueTask SignUpAsync(IAuthClient client, CoreUserIdentifierType type, HttpContext context, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> WebInitCallAsync(InitCallRQ rq, string identifier);
        ValueTask<RegisterUserData?> ViewRegisterDataAsync(CancellationToken cancellationToken = default);
        ValueTask<AppTokenData?> OAuthCreateTokenAsync(AuthCreateTokenRQ rq, CancellationToken cancellationToken = default);
        ValueTask<AppTokenData?> OAuthRefreshTokenAsync(AuthRefreshTokenRQ rq, CancellationToken cancellationToken = default);
        ValueTask<(IActionResult result, string? newRefreshToken)> OAuthRefreshTokenResultAsync(AuthRefreshTokenRQ rq, CancellationToken cancellationToken = default);
        Task OAuthUserInfoAsync(HttpResponse? response, CancellationToken cancellationToken = default);
        ValueTask<AppTokenData?> SwitchOrgAsync(SwitchOrgProxyRQ rq, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> ResetPasswordAsync(ResetPasswordRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> SignoutAsync(string token);

        Task<IActionResult> ValidateEmailCallbackAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        Task<IActionResult> ValidateEmailRegistrationAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        Task<IActionResult> ValidateMobileCallbackAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        Task<IActionResult> ValidateMobileRegistrationAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default);
    }
}