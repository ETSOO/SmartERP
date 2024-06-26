using com.etsoo.CoreFramework.Models;
using com.etsoo.Web;
using com.etsoo.WebUtils;
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

            return builder;
        }
    }
}
