using Platform.Server.Endpoints.Public.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.Public
{
    /// <summary>
    /// Public service APIs
    /// 公共服务API
    /// </summary>
    public static class Public
    {
        public static RouteGroupBuilder MapPublic(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Public").AllowAnonymous();

            g.MapPost("MobileQRCode", (IPublicService service, MobileQRCodeRQ rq, CancellationToken cancellationToken) =>
            {
                return service.MobileQRCodeAsync(rq, cancellationToken);
            }).WithDescription("Get mobile QRCode image Base64 string / 获取移动端QRCode图片的Base64字符串");

            return builder;
        }
    }
}
