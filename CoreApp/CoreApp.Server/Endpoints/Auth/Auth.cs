using com.etsoo.ServiceApp.SmartERP;
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

            g.MapGet("GetLogInUrl", (ISEAuthService service, HttpContext context, string region, string device)
                => service.GetLogInUrlResult(context.UserAgent(), region + device)
                ).WithDescription("Get log in URL / 获取登录地址");

            g.MapGet("LogIn", (ISEAuthService service, HttpContext context, CancellationToken cancellation)
                => service.AuthLogInAsync(context, cancellation)
                ).WithDescription("OAuth2 log in / OAuth2 登录");

            return builder;
        }
    }
}
