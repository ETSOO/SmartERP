using com.etsoo.MicrosoftApi.Auth;
using Platform.Server.Database.Models;
using Platform.Server.Services;

namespace Platform.Server.OAuth2
{
    public static class MicrosoftAuth
    {
        public static RouteGroupBuilder MapMicrosoft(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Microsoft");

            g.MapGet("GetLogInUrl", (IAuthService service, IMicrosoftAuthClient client, HttpRequest request, string device)
                => service.GetLogInUrl(client, request.Headers.UserAgent, device)
                ).WithDescription("Microsoft OAuth2 get log in URL / 微软 OAuth2 获取登录地址");

            g.MapGet("GetSignUpUrl", (IAuthService service, IMicrosoftAuthClient client, HttpRequest request, string device)
                => service.GetSignUpUrl(client, request.Headers.UserAgent, device)
                ).WithDescription("Microsoft OAuth2 get sign up URL / 微软 OAuth2 获取注册地址");

            g.MapGet("LogIn", (IAuthService service, IMicrosoftAuthClient client, HttpContext context, CancellationToken cancellation)
                => service.LogInAsync(client, CoreUserIdentifierType.Microsoft, context, cancellation)
                ).WithDescription("Microsoft OAuth2 log in / 微软 OAuth2 登录");

            g.MapGet("SignUp", (IAuthService service, IMicrosoftAuthClient client, HttpContext context, CancellationToken cancellation)
                => service.SignUpAsync(client, CoreUserIdentifierType.Microsoft, context, cancellation)
                ).WithDescription("Microsoft OAuth2 sign up / 微软 OAuth2 注册");

            return builder;
        }
    }
}
