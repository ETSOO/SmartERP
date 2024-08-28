using com.etsoo.GoogleApi.Auth;
using Platform.Server.Database.Models;
using Platform.Server.Services;

namespace Platform.Server.OAuth2
{
    public static class GoogleAuth
    {
        public static RouteGroupBuilder MapGoogle(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Google");

            g.MapGet("GetLogInUrl", (IAuthService service, IGoogleAuthClient client, HttpRequest request, string region, string device)
                => service.GetLogInUrl(client, request.Headers.UserAgent, region + device)
                ).WithDescription("Google OAuth2 get log in URL / 谷歌 OAuth2 获取登录地址");

            g.MapGet("GetSignUpUrl", (IAuthService service, IGoogleAuthClient client, HttpRequest request, string region, string device)
                => service.GetSignUpUrl(client, request.Headers.UserAgent, region + device)
                ).WithDescription("Google OAuth2 get sign up URL / 谷歌 OAuth2 获取注册地址");

            g.MapGet("LogIn", (IAuthService service, IGoogleAuthClient client, HttpContext context, CancellationToken cancellation)
                => service.LogInAsync(client, CoreUserIdentifierType.Google, context, cancellation)
                ).WithDescription("Google OAuth2 log in / 谷歌 OAuth2 登录");

            g.MapGet("SignUp", (IAuthService service, IGoogleAuthClient client, HttpContext context, CancellationToken cancellation)
                => service.SignUpAsync(client, CoreUserIdentifierType.Google, context, cancellation)
                ).WithDescription("Google OAuth2 sign up / 谷歌 OAuth2 注册");

            return builder;
        }
    }
}
