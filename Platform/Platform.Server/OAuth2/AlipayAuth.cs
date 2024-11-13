using com.etsoo.AlipayApi;
using Platform.Server.Services;
using PlatformShared.Database.Models;

namespace Platform.Server.OAuth2
{
    /// <summary>
    /// Alipay OAuth2
    /// 支付宝 OAuth2
    /// </summary>
    public static class AlipayAuth
    {
        public static RouteGroupBuilder MapAlipay(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Alipay");

            g.MapGet("GetLogInUrl", (IAuthService service, IAlipayClient client, HttpRequest request, string region, string device)
                => service.GetLogInUrl(client, request.Headers.UserAgent, region + device)
                ).WithDescription("Alipay OAuth2 get log in URL / 支付宝 OAuth2 获取登录地址").WithTags("OAuth2");

            g.MapGet("GetSignUpUrl", (IAuthService service, IAlipayClient client, HttpRequest request, string region, string device)
                => service.GetSignUpUrl(client, request.Headers.UserAgent, region + device)
                ).WithDescription("Alipay OAuth2 get sign up URL / 支付宝 OAuth2 获取注册地址").WithTags("OAuth2");

            g.MapGet("LogIn", (IAuthService service, IAlipayClient client, HttpContext context, CancellationToken cancellation)
                => service.LogInAsync(client, CoreUserIdentifierType.Alipay, context, cancellation)
                ).WithDescription("Alipay OAuth2 log in / 支付宝 OAuth2 登录").WithTags("OAuth2");

            g.MapGet("SignUp", (IAuthService service, IAlipayClient client, HttpContext context, CancellationToken cancellation)
                => service.SignUpAsync(client, CoreUserIdentifierType.Alipay, context, cancellation)
                ).WithDescription("Alipay OAuth2 sign up / 支付宝 OAuth2 注册").WithTags("OAuth2");

            return builder;
        }
    }
}
