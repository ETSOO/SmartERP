using com.etsoo.AlipayApi;
using Microsoft.AspNetCore.Mvc;
using Platform.Server.Services;

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

            g.MapGet("GetServerAuthUrl", (IAuthService service, IAlipayClient client, HttpContext context, [FromQuery(Name = "device")] string device) =>
            {


                return client.GetServerAuthUrl("abc", "auth_user");
            });

            g.MapGet("SignIn", async (IAuthService service, IAlipayClient client, HttpContext context, HttpRequest request) =>
            {

                var (result, userInfo) = await client.GetUserInfoAsync(request, "abc");
                if (result.Ok && userInfo != null)
                {
                    context.Response.Redirect("https://localhost:9002/login/register/");
                }
            }).WithDescription("Alipay OAuth2 sign in / 支付宝 OAuth2 登录");

            return builder;
        }
    }
}
