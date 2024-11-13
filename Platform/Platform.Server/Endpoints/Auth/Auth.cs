using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Web;
using com.etsoo.WebUtils;
using Platform.Server.Dto.Auth;
using Platform.Server.Endpoints.Auth.RQ;
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

            g.MapPut("ApiRefreshToken", (IAuthService service, ApiRefreshTokenRQ rq, CancellationToken cancellation)
                => service.ApiRefreshTokenAsync(rq, cancellation)
                ).WithDescription("API refresh token / 接口刷新令牌").WithTags("Auth");

            g.MapPut("CompleteRegister", async (IAuthService service, IHttpContextAccessor accessor, CompleteRegisterRQ rq, CancellationToken cancellationToken) =>
            {
                var (result, refreshToken) = await service.CompleteRegisterAsync(rq, accessor.UserAgent(), cancellationToken);

                if (result.Ok && refreshToken != null)
                {
                    MinimalApiUtils.OutputRefreshToken(accessor, refreshToken);
                }

                return result;
            }).WithDescription("Complete registration / 完成注册").WithTags("Auth");

            g.MapPut("WebInitCall", async (IAuthService service, IHttpContextAccessor accessor, InitCallRQ rq) =>
            {
                // Device check
                if (!MinimalApiUtils.CheckDevice(accessor.UserAgent(), out var checkResult, out var parser))
                {
                    return checkResult;
                }

                // Result
                return await service.WebInitCallAsync(rq, parser.ToShortName());
            }).WithDescription("Init call / 初始化调用").WithTags("Auth");

            g.MapPut("ChangePassword", async (IAuthService service, IHttpContextAccessor accessor, ChangePasswordRQ rq, CancellationToken cancellationToken) =>
            {
                // Device check
                if (!MinimalApiUtils.CheckDevice(accessor.UserAgent(), out var checkResult, out var parser))
                {
                    return checkResult;
                }

                // Data
                var data = new ChangePasswordDto("", "");

                // Result
                return await service.ChangePasswordAsync(data, cancellationToken);
            }).WithDescription("Change password / 修改密码").RequireAuthorization().WithTags("Auth");

            g.MapPost("Login", async (IAuthService service, IHttpContextAccessor accessor, LoginRQ rq, CancellationToken cancellationToken) =>
            {
                var (result, refreshToken) = await service.LoginWithPwdAsync(rq, accessor.UserAgent(), cancellationToken);

                if (result.Ok && refreshToken != null)
                {
                    MinimalApiUtils.OutputRefreshToken(accessor, refreshToken);
                }

                return result;
            }).WithDescription("User login with password / 用户使用密码登录").WithTags("Auth");

            g.MapPost("LoginId", async (IAuthService service, IHttpContextAccessor accessor, LoginIdRQ rq, CancellationToken cancellationToken) =>
            {
                // Check device
                if (!service.CheckDevice(accessor.UserAgent(), rq.DeviceId, out var checkResult, out var cd))
                {
                    return checkResult;
                }

                var deviceCore = cd.Value.DeviceCore;

                var id = service.DecryptDeviceData(rq.Id, deviceCore);
                if (string.IsNullOrEmpty(id) || id.Length < 6)
                {
                    return ApplicationErrors.NoValidData.AsResult();
                }

                var (result, _) = await service.LoginIdAsync(id, rq.Region, cancellationToken);

                return result;
            }).WithDescription("Check user login id / 检查用户登录编号").WithTags("Auth");

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
                    UserAgent = accessor.UserAgent(),
                    Token = token
                };

                var (result, newRefeshToken) = await service.RefreshTokenAsync(data, cancellationToken);

                if (result.Ok && newRefeshToken != null)
                {
                    MinimalApiUtils.OutputRefreshToken(accessor, newRefeshToken);
                }

                return result;
            }).WithDescription("Refresh token / 刷新令牌").WithTags("Auth");

            g.MapPut("SendEmail", async (IAuthService service, IHttpContextAccessor accessor, EmailCodeRQ rq, CancellationToken cancellationToken) =>
            {
                // Check device
                if (!service.CheckDevice(accessor.UserAgent(), rq.DeviceId, out var checkResult, out var cd))
                {
                    return checkResult;
                }

                var deviceCore = cd.Value.DeviceCore;

                var email = service.DecryptDeviceData(rq.Email, deviceCore);
                if (email == null)
                {
                    return ApplicationErrors.NoValidData.AsResult("Email");
                }

                var data = new SendEmailData
                {
                    Action = rq.Action,
                    Email = email,
                    Region = rq.Region,
                    TimeZone = rq.TimeZone
                };

                return await service.SendEmailAsync(data, cancellationToken);
            }).WithDescription("Send Email code / 发送电子邮箱验证码").WithTags("Auth");

            g.MapPut("SendSMS", async (IAuthService service, IHttpContextAccessor accessor, SMSCodeRQ rq, CancellationToken cancellationToken) =>
            {
                // Check device
                if (!service.CheckDevice(accessor.UserAgent(), rq.DeviceId, out var checkResult, out var cd))
                {
                    return checkResult;
                }

                var deviceCore = cd.Value.DeviceCore;

                var mobile = service.DecryptDeviceData(rq.Mobile, deviceCore);
                if (mobile == null)
                {
                    return ApplicationErrors.NoValidData.AsResult("Mobile");
                }

                var data = new SendSMSData
                {
                    Action = rq.Action,
                    Mobile = mobile,
                    Region = rq.Region
                };

                return await service.SendSMSAsync(data, cancellationToken);
            }).WithDescription("Send SMS code / 发送短信验证码").WithTags("Auth");

            g.MapPut("ValidateEmailRegistration", async (IAuthService service, IHttpContextAccessor accessor, CodeValidateRQ rq, CancellationToken cancellationToken) =>
            {
                // Check device
                if (!service.CheckDevice(accessor.UserAgent(), rq.DeviceId, out var checkResult, out var cd))
                {
                    return checkResult;
                }

                var deviceCore = cd.Value.DeviceCore;

                var code = service.DecryptDeviceData(rq.Code, deviceCore);
                if (code == null)
                {
                    return ApplicationErrors.NoValidData.AsResult("Code");
                }

                var data = new ValidateCodeData
                {
                    Code = code,
                    Id = rq.Id
                };

                return await service.ValidateEmailRegistrationAsync(data, cancellationToken);
            }).WithDescription("Validate email registration code / 验证电子邮箱注册验证码").WithTags("Auth");

            g.MapPut("ValidateMobileRegistration", async (IAuthService service, IHttpContextAccessor accessor, CodeValidateRQ rq, CancellationToken cancellationToken) =>
            {
                // Check device
                if (!service.CheckDevice(accessor.UserAgent(), rq.DeviceId, out var checkResult, out var cd))
                {
                    return checkResult;
                }

                var deviceCore = cd.Value.DeviceCore;

                var code = service.DecryptDeviceData(rq.Code, deviceCore);
                if (code == null)
                {
                    return ApplicationErrors.NoValidData.AsResult("Code");
                }

                var data = new ValidateCodeData
                {
                    Code = code,
                    Id = rq.Id
                };

                return await service.ValidateMobileRegistrationAsync(data, cancellationToken);
            }).WithDescription("Validate mobile registration code / 验证手机注册验证码").WithTags("Auth");

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

            g.MapPost("AuthRequest", (IAuthService service, AuthRequest rq, CancellationToken cancellationToken) =>
            {
                return service.AuthRequestAsync(rq, cancellationToken);
            }).WithDescription("User authorization request / 用户授权请求").RequireAuthorization().WithTags("Auth");

            g.MapPut("SwitchOrg", async (IAuthService service, SwitchOrgRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) =>
            {
                var (result, newRefeshToken) = await service.SwitchOrgAsync(rq, cancellationToken);

                if (result.Ok && newRefeshToken != null)
                {
                    MinimalApiUtils.OutputRefreshToken(accessor, newRefeshToken);
                }

                return result;
            }).WithDescription("User switch organization / 用户切换机构").RequireAuthorization().WithTags("Auth");

            g.MapPut("Signout", async (IAuthService service, SignoutRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) =>
            {
                // Check device
                if (!service.CheckDevice(accessor.UserAgent(), rq.DeviceId, out var checkResult, out var cd))
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

            return builder;
        }
    }
}
