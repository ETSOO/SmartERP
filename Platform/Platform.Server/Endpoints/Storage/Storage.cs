using com.etsoo.Utils.Storage;

namespace Platform.Server.Endpoints.Storage
{
    /// <summary>
    /// Storage service APIs
    /// 存储服务API
    /// </summary>
    public static class Storage
    {
        public static RouteGroupBuilder MapStorage(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Storage").AllowAnonymous();

            g.MapGet("OrgAvatar/{folder}/{file}", async (IStorage storage, IHttpContextAccessor accessor, string folder, string file, CancellationToken cancellationToken) =>
            {
                await using var stream = await storage.ReadAsync($"/OrgAvatar/{folder}/{file}", cancellationToken);
                if (stream != null && accessor.HttpContext != null)
                    await stream.CopyToAsync(accessor.HttpContext.Response.Body, cancellationToken);
            }).WithDescription("Read organization avatar / 读取机构头像").WithTags("Storage");

            g.MapGet("Resources/{path}", async (IStorage storage, IHttpContextAccessor accessor, string path, CancellationToken cancellationToken) =>
            {
                await using var stream = await storage.ReadAsync($"/Resources/{path}", cancellationToken);
                if (stream != null && accessor.HttpContext != null)
                    await stream.CopyToAsync(accessor.HttpContext.Response.Body, cancellationToken);
            }).WithDescription("Read resources / 读取资源").WithTags("Storage");

            g.MapGet("UserAvatar/{folder}/{file}", async (IStorage storage, IHttpContextAccessor accessor, string folder, string file, CancellationToken cancellationToken) =>
            {
                await using var stream = await storage.ReadAsync($"/UserAvatar/{folder}/{file}", cancellationToken);
                if (stream != null && accessor.HttpContext != null)
                    await stream.CopyToAsync(accessor.HttpContext.Response.Body, cancellationToken);
            }).WithDescription("Read user avatar / 读取用户头像").WithTags("Storage");

            return builder;
        }
    }
}
