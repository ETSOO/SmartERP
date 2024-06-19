namespace Platform.Server.OAuth2
{
    public static class GoogleOAuth2
    {
        public static RouteGroupBuilder MapGoogle(this RouteGroupBuilder g)
        {
            g.MapGroup("Google")
                .MapGet("SignIn", (HttpRequest request) =>
                {

                }).WithDescription("");

            return g;
        }
    }
}
