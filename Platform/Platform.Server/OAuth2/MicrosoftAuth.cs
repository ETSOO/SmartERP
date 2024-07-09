using com.etsoo.MicrosoftApi.Auth;

namespace Platform.Server.OAuth2
{
    public static class MicrosoftAuth
    {
        public static RouteGroupBuilder MapMicrosoft(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Microsoft");

            g.MapGet("GetServerAuthUrl", (IMicrosoftAuthClient client, HttpContext context) =>
            {
                return client.GetServerAuthUrl("abc", "openid profile email https://graph.microsoft.com/User.Read", true);
            });

            g.MapGet("SignIn", async (IMicrosoftAuthClient client, HttpContext context, HttpRequest request) =>
            {
                var (result, userInfo) = await client.GetUserInfoAsync(request, "abc");
                return result;
            }).WithDescription("Microsoft OAuth2 sign in / 微软 OAuth2 登录");

            return builder;
        }
    }
}
