using com.etsoo.GoogleApi.Auth;

namespace Platform.Server.OAuth2
{
    public static class GoogleAuth
    {
        public static RouteGroupBuilder MapGoogle(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Google");

            g.MapGet("GetServerAuthUrl", (IGoogleAuthClient client, HttpContext context) =>
            {
                return client.GetServerAuthUrl("abc", "openid https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile");
            });

            g.MapGet("SignIn", async (IGoogleAuthClient client, HttpContext context, HttpRequest request) =>
            {
                var (result, userInfo) = await client.GetUserInfoAsync(request, "abc");
                return result;
            }).WithDescription("Google OAuth2 sign in / 谷歌 OAuth2 登录");

            return builder;
        }
    }
}
