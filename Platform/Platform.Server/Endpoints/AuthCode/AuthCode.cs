namespace Platform.Server.Endpoints.AuthCode
{
    /// <summary>
    /// Authentication code
    /// 认证验证码
    /// </summary>
    public static class AuthCode
    {
        public static RouteGroupBuilder MapAuthCode(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("AuthCode").AllowAnonymous();

            return builder;
        }
    }
}
