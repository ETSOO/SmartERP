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
        private static void OutputRefreshToken(IHttpContextAccessor accessor, string refreshToken)
        {
            accessor.HttpContext?.Response.Headers.Append(Constants.RefreshTokenHeader, refreshToken);
        }

        public static RouteGroupBuilder MapAuth(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Auth").AllowAnonymous();

            g.MapPut("CompleteRegister", async (IAuthService service, IHttpContextAccessor accessor, CompleteRegisterRQ rq, CancellationToken cancellationToken) =>
            {
                var data = new CompleteRegisterData
                {
                    UserAgent = accessor.UserAgent(),
                    DeviceId = rq.DeviceId,
                    Password = rq.Password,
                    Name = rq.Name,
                    Region = rq.Region
                };

                var (result, refreshToken) = await service.CompleteRegisterAsync(data, cancellationToken);

                if (result.Ok && refreshToken != null)
                {
                    OutputRefreshToken(accessor, refreshToken);
                }

                return result;
            });

            g.MapPut("WebInitCall", async (IAuthService service, IHttpContextAccessor accessor, InitCallRQ rq) =>
            {
                // Device check
                if (!MinimalApiUtils.CheckDevice(accessor.UserAgent(), out var checkResult, out var parser))
                {
                    return checkResult;
                }

                // Result
                var initResult = await service.WebInitCallAsync(rq, parser.ToShortName());

                return initResult;
            }).WithDescription("Init call / 初始化调用");

            g.MapPost("Login", async (IAuthService service, IHttpContextAccessor accessor, LoginRQ rq, CancellationToken cancellationToken) =>
            {
                var data = new LoginData
                {
                    Id = rq.Id,
                    Password = rq.Pwd,
                    DeviceId = rq.DeviceId,
                    UserAgent = accessor.UserAgent(),
                    Region = rq.Region,
                    Timezone = rq.Timezone
                };

                var (result, refreshToken) = await service.LoginWithPwdAsync(data, cancellationToken);

                if (result.Ok && refreshToken != null)
                {
                    OutputRefreshToken(accessor, refreshToken);
                }

                return result;
            }).WithDescription("Check user login id / 检查用户登录编号");

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
            }).WithDescription("Check user login id / 检查用户登录编号");

            g.MapPut("RefreshToken", async (IAuthService service, IHttpContextAccessor accessor, RefreshTokenRQ rq, CancellationToken cancellationToken) =>
            {
                // Token
                string? token;
                if (accessor.HttpContext?.Request.Headers.TryGetValue(Constants.RefreshTokenHeader, out var value) is true)
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
                    Region = rq.Region,
                    DeviceId = rq.DeviceId,
                    UserAgent = accessor.UserAgent(),
                    Token = token,
                    Password = rq.Pwd
                };

                var (result, newRefeshToken) = await service.RefreshTokenAsync(data, token, cancellationToken);

                if (result.Ok && newRefeshToken != null)
                {
                    OutputRefreshToken(accessor, newRefeshToken);
                }

                return result;
            }).WithDescription("Refresh token / 刷新令牌");

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
            }).WithDescription("Send Email code / 发送电子邮箱验证码");

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
            }).WithDescription("Send SMS code / 发送短信验证码");

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
            }).WithDescription("Validate email registration code / 验证电子邮箱注册验证码");

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
            }).WithDescription("Validate mobile registration code / 验证手机注册验证码");

            g.MapGet("ViewRegisterData", (IAuthService service, CancellationToken cancellationToken) =>
            {
                return service.ViewRegisterDataAsync(cancellationToken);
            }).WithDescription("View register data / 查看注册数据");

            return builder;
        }
    }
}
