using com.etsoo.WeiXin.Auth;
using Platform.Server.Services;
using PlatformShared.Database.Models;

namespace Platform.Server.OAuth2
{
    /// <summary>
    /// Wechat OAuth2
    /// 微信 OAuth2
    /// </summary>
    public static class WechatAuth
    {
        public static RouteGroupBuilder MapWechat(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Wechat");

            g.MapGet("GetLogInUrl", (IAuthService service, IWechatAuthClient client, HttpRequest request, string region, string device)
                => service.GetLogInUrl(client, request.Headers.UserAgent, region + device)
                ).WithDescription("Wechat OAuth2 get log in URL / 微信 OAuth2 获取登录地址").WithTags("OAuth2");

            g.MapGet("GetSignUpUrl", (IAuthService service, IWechatAuthClient client, HttpRequest request, string region, string device)
                => service.GetSignUpUrl(client, request.Headers.UserAgent, region + device)
                ).WithDescription("Wechat OAuth2 get sign up URL / 微信 OAuth2 获取注册地址").WithTags("OAuth2");

            g.MapGet("LogIn", (IAuthService service, IWechatAuthClient client, HttpContext context, CancellationToken cancellation)
                => service.LogInAsync(client, CoreUserIdentifierType.Wechat, context, cancellation)
                ).WithDescription("Wechat OAuth2 log in / 微信 OAuth2 登录").WithTags("OAuth2");

            g.MapGet("SignUp", (IAuthService service, IWechatAuthClient client, HttpContext context, CancellationToken cancellation)
                => service.SignUpAsync(client, CoreUserIdentifierType.Wechat, context, cancellation)
                ).WithDescription("Wechat OAuth2 sign up / 微信 OAuth2 注册").WithTags("OAuth2");

            return builder;
        }
    }
}
