using Microsoft.AspNetCore.Mvc;
using Platform.Server.Services;

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

            g.MapGet("EditorStyles", (IStorageService service, CancellationToken cancellationToken) =>
                service.EditorStylesAsync(cancellationToken)
            ).WithDescription("Editor styles / 编辑器样式").WithTags("Storage");

            g.MapGet("OrgAvatar/{folder}/{file}", (IStorageService service, string folder, string file, CancellationToken cancellationToken) =>
                service.DownloadFileAsync($"/OrgAvatar/{folder}/{file}", cancellationToken)
            ).WithDescription("Read organization avatar / 读取机构头像").WithTags("Storage");

            g.MapGet("OUAvatar/{folder}/{file}", (IStorageService service, string folder, string file, CancellationToken cancellationToken) =>
                service.DownloadFileAsync($"/OUAvatar/{folder}/{file}", cancellationToken)
            ).WithDescription("Read organization user avatar / 读取机构用户头像").WithTags("Storage");

            g.MapGet("ProfileAttachment/{id:long}", (IStorageService service, [FromRoute] long id, [FromQuery] long timestamp, [FromQuery] string key, CancellationToken cancellationToken) =>
                service.ProfileAttachmentAsync(id, timestamp, key, cancellationToken)
            ).WithDescription("Read profile attachment / 读取档案附件").WithTags("Storage");

            g.MapGet("Resources/{*path}", (IStorageService service, string path, CancellationToken cancellationToken) =>
                service.DownloadFileAsync($"/Resources/{path}", cancellationToken)
            ).WithDescription("Read resources / 读取资源").WithTags("Storage");

            g.MapGet("UserAvatar/{folder}/{file}", (IStorageService service, string folder, string file, CancellationToken cancellationToken) =>
                service.DownloadFileAsync($"/UserAvatar/{folder}/{file}", cancellationToken)
            ).WithDescription("Read user avatar / 读取用户头像").WithTags("Storage");

            return builder;
        }
    }
}
