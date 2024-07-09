using com.etsoo.WeiXin.Auth;

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

            g.MapGet("GetServerAuthUrl", (IWechatAuthClient client, HttpContext context) =>
            {
                return client.GetServerAuthUrl("abc", "snsapi_login");
            });

            g.MapGet("SignIn", async (IWechatAuthClient client, HttpContext context, HttpRequest request) =>
            {
                var (result, userInfo) = await client.GetUserInfoAsync(request, "abc");
                return result;
            }).WithDescription("Wechat OAuth2 sign in / 微信 OAuth2 登录");

            return builder;
        }
    }
}
