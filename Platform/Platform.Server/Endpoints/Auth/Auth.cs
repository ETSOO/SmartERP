using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Web;
using com.etsoo.WebUtils;
using Platform.Server.Application;
using Platform.Server.Endpoints.Auth.RQ;
using Platform.Server.Endpoints.AuthCode.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.Auth
{
    /// <summary>
    /// User authentication service APIs
    /// 用户认证服务API
    /// </summary>
    public static class Auth
    {
        public static RouteGroupBuilder MapAuth(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Auth").AllowAnonymous();

            g.MapPost("AdminSupport", async (IAuthService service, IHttpContextAccessor accessor, AdminSupportRQ rq, CancellationToken cancellationToken) =>
            {
                // Device check
                if (!MinimalApiUtils.CheckDevice(accessor.UserAgent, out var checkResult, out var parser))
                {
                    return checkResult;
                }

                // Result
                return await service.AdminSupportAsync(rq, parser.ToShortName(), cancellationToken);
            }).WithDescription("Admin support / 管理员支持").RequireAuthorization().WithTags("Auth");

            g.MapPut("ApiRefreshToken", (IAuthService service, ApiRefreshTokenRQ rq, CancellationToken cancellation)
                => service.ApiRefreshTokenAsync(rq, cancellation)
                ).WithDescription("API refresh token / 接口刷新令牌").WithTags("Auth");

            g.MapPut("CompleteRegister", async (IAuthService service, IHttpContextAccessor accessor, CompleteRegisterRQ rq, CancellationToken cancellationToken) =>
            {
                var (result, refreshToken) = await service.CompleteRegisterAsync(rq, accessor.UserAgent, cancellationToken);

                if (result.Ok && refreshToken != null)
                {
                    MinimalApiUtils.OutputRefreshToken(accessor, refreshToken);
                }

                return result;
            }).WithDescription("Complete registration / 完成注册").WithTags("Auth");

            g.MapPut("WebInitCall", async (IAuthService service, IHttpContextAccessor accessor, MyAppConfiguration config, InitCallRQ rq) =>
            {
                // Device check
                if (!MinimalApiUtils.CheckDevice(accessor.UserAgent, out var checkResult, out var parser))
                {
                    return checkResult;
                }

                // Result
                var result = await service.WebInitCallAsync(rq, parser.ToShortName());

                // Additional data
                if (result.Ok)
                {
                    result.Data.Add(nameof(config.AuthClients), config.AuthClients);
                }

                return result;
            }).WithDescription("Init call / 初始化调用").WithTags("Auth");

            g.MapPut("ChangePassword", (IAuthService service, IHttpContextAccessor accessor, ChangePasswordRQ rq, CancellationToken cancellationToken) => service.ChangePasswordAsync(rq, accessor.UserAgent, cancellationToken))
                .WithDescription("Change password / 修改密码").RequireAuthorization().WithTags("Auth");

            g.MapPost("CheckUserIdentifier", (IAuthService service, IHttpContextAccessor accessor, CheckUserIdentifierRQ rq, CancellationToken cancellationToken) => service.CheckUserIdentifierAsync(rq, accessor.UserAgent, cancellationToken))
                .WithDescription("Check user identifier exists / 检查用户标识是否存在").WithTags("Auth").RequireRateLimiting("PII"); ;

            g.MapPost("Login", async (IAuthService service, IHttpContextAccessor accessor, LoginRQ rq, CancellationToken cancellationToken) =>
            {
                var (result, refreshToken) = await service.LoginWithPwdAsync(rq, accessor.UserAgent, cancellationToken);

                if (result.Ok && refreshToken != null)
                {
                    MinimalApiUtils.OutputRefreshToken(accessor, refreshToken);
                }

                return result;
            }).WithDescription("User login with password / 用户使用密码登录").WithTags("Auth");

            g.MapPost("LoginId", (IAuthService service, IHttpContextAccessor accessor, LoginIdRQ rq, CancellationToken cancellationToken) => service.LoginIdAsync(rq, accessor.UserAgent, cancellationToken))
                .WithDescription("Check user login id / 检查用户登录编号").WithTags("Auth");

            g.MapPut("RefreshToken", async (IAuthService service, IHttpContextAccessor accessor, RefreshTokenRQ rq, CancellationToken cancellationToken) =>
            {
                // Token
                string? token;
                if (accessor.HttpContext?.Request.Headers.TryGetValue(Constants.RefreshTokenHeaderName, out var value) is true)
                {
                    token = value.ToString();
                }
                else
                {
                    return ApplicationErrors.NoValidData.AsResult("Token");
                }

                if (string.IsNullOrEmpty(token))
                {
                    return ApplicationErrors.NoValidData.AsResult("Token");
                }

                var data = new RefreshTokenData
                {
                    DeviceId = rq.DeviceId,
                    UserAgent = accessor.UserAgent,
                    Token = token,
                    TimeZone = rq.TimeZone
                };

                var (result, newRefeshToken) = await service.RefreshTokenAsync(data, cancellationToken);

                if (result.Ok && newRefeshToken != null)
                {
                    MinimalApiUtils.OutputRefreshToken(accessor, newRefeshToken);
                }

                return result;
            }).WithDescription("Refresh token / 刷新令牌").WithTags("Auth");

            g.MapGet("ViewRegisterData", (IAuthService service, CancellationToken cancellationToken) => service.ViewRegisterDataAsync(cancellationToken))
                .WithDescription("View register data / 查看注册数据").WithTags("Auth");

            g.MapPost("OAuthCreateToken", (IAuthService service, AuthCreateTokenRQ rq, CancellationToken cancellationToken) => service.OAuthCreateTokenAsync(rq, cancellationToken))
                .WithDescription("OAuth create token / OAuth 创建令牌").WithTags("Auth");

            g.MapPost("OAuthRefreshToken", (IAuthService service, AuthRefreshTokenRQ rq, CancellationToken cancellationToken) => service.OAuthRefreshTokenAsync(rq, cancellationToken))
                .WithDescription("OAuth refresh token / OAuth 刷新令牌").WithTags("Auth");

            g.MapPost("OAuthRefreshTokenResult", async (IAuthService service, AuthRefreshTokenRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) =>
            {
                var (result, newRefeshToken) = await service.OAuthRefreshTokenResultAsync(rq, cancellationToken);

                if (result.Ok && newRefeshToken != null)
                {
                    MinimalApiUtils.OutputRefreshToken(accessor, newRefeshToken);
                }

                return result;
            }).WithDescription("OAuth refresh token result / OAuth 刷新令牌结果").WithTags("Auth");

            g.MapGet("OAuthUserInfo", (IAuthService service, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.OAuthUserInfoAsync(accessor.HttpContext?.Response, cancellationToken))
                .WithDescription("OAuth get user information / OAuth 获取用户信息").RequireAuthorization().WithTags("Auth");

            g.MapPost("AuthRequest", (IAuthService service, AuthRequest rq, CancellationToken cancellationToken) => service.AuthRequestAsync(rq, cancellationToken))
                .WithDescription("User authorization request / 用户授权请求").RequireAuthorization().WithTags("Auth");

            g.MapPut("SwitchOrg", (IAuthService service, SwitchOrgProxyRQ rq, CancellationToken cancellationToken) => service.SwitchOrgAsync(rq, cancellationToken))
                .WithDescription("User switch organization / 用户切换机构").RequireAuthorization().WithTags("Auth");

            g.MapPut("ResetPassword", (IAuthService service, IHttpContextAccessor accessor, ResetPasswordRQ rq, CancellationToken cancellationToken) => service.ResetPasswordAsync(rq, accessor.UserAgent, cancellationToken))
                .WithDescription("Reset password / 重置密码").WithTags("Auth");

            g.MapPut("Signout", async (IAuthService service, SignoutRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) =>
            {
                // Check device
                if (!service.CheckDevice(accessor.UserAgent, rq.DeviceId, out var checkResult, out var cd))
                {
                    return checkResult;
                }

                var deviceCore = cd.Value.DeviceCore;

                var token = service.DecryptDeviceData(rq.Token, deviceCore);
                if (token == null)
                {
                    return ApplicationErrors.NoValidData.AsResult("Token");
                }

                return await service.SignoutAsync(token);
            }).WithDescription("User signout / 用户退出").WithTags("Auth");

            g.MapPut("ValidateEmailCallback", (IAuthService service, IHttpContextAccessor accessor, CodeValidateRQ rq, CancellationToken cancellationToken) => service.ValidateEmailCallbackAsync(rq, accessor.UserAgent, cancellationToken))
                .WithDescription("Validate email callback password code / 验证电子邮箱找回密码验证码").WithTags("Auth");

            g.MapPut("ValidateEmailRegistration", (IAuthService service, IHttpContextAccessor accessor, CodeValidateRQ rq, CancellationToken cancellationToken) => service.ValidateEmailRegistrationAsync(rq, accessor.UserAgent, cancellationToken))
                .WithDescription("Validate email registration code / 验证电子邮箱注册验证码").WithTags("Auth");

            g.MapPut("ValidateMobileCallback", (IAuthService service, IHttpContextAccessor accessor, CodeValidateRQ rq, CancellationToken cancellationToken) => service.ValidateMobileCallbackAsync(rq, accessor.UserAgent, cancellationToken))
                .WithDescription("Validate mobile callback password code / 验证手机找回密码验证码").WithTags("Auth");

            g.MapPut("ValidateMobileRegistration", (IAuthService service, IHttpContextAccessor accessor, CodeValidateRQ rq, CancellationToken cancellationToken) => service.ValidateMobileRegistrationAsync(rq, accessor.UserAgent, cancellationToken))
                .WithDescription("Validate mobile registration code / 验证手机注册验证码").WithTags("Auth");

            return builder;
        }
    }
}
