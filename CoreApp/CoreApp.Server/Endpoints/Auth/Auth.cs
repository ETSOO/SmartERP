using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Web;
using com.etsoo.WebUtils;

namespace CoreApp.Server.Endpoints.Auth
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

            g.MapGet("GetLogInUrl", (ISEAuthService service, HttpContext context, string region, string? device)
                => service.GetLogInUrlResult(context.UserAgent(), region + device)
                ).WithDescription("Get log in URL / 获取登录地址");

            g.MapGet("LogIn", (ISEAuthService service, HttpContext context, CancellationToken cancellation)
                => service.AuthLogInAsync(context, cancellation)
                ).WithDescription("OAuth2 log in / OAuth2 登录");

            g.MapPut("ApiRefreshToken", (ISEAuthService service, ApiRefreshTokenRQ rq, CancellationToken cancellation)
                => service.ApiRefreshTokenAsync(rq, cancellation)
                ).WithDescription("API refresh token / 接口刷新令牌");

            g.MapPut("ExchangeToken", (ISEAuthService service, ApiTokenRQ rq, CancellationToken cancellation)
                => service.ExchangeTokenAsync(rq.Token, cancellation)
                ).WithDescription("API exchange token with core system / 接口和核心系统交换令牌");

            g.MapPut("RefreshToken", async (ISEAuthService service, IHttpContextAccessor accessor, RefreshTokenRQ rq, CancellationToken cancellationToken) =>
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
            }).WithDescription("Refresh token / 刷新令牌");

            g.MapPut("Signout", async (ISEAuthService service, SignoutRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) =>
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
            }).WithDescription("User signout / 用户退出");

            return builder;
        }
    }
}
