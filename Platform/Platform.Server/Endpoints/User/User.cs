using com.etsoo.WebUtils;
using Microsoft.AspNetCore.Mvc;
using Platform.Server.Endpoints.User.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.User
{
    /// <summary>
    /// User service APIs
    /// 用户服务API
    /// </summary>
    public static class User
    {
        public static RouteGroupBuilder MapUser(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("User");

            g.MapPost("AuditHistory", (IUserService service, IHttpContextAccessor accessor, AuditHistoryRQ rq, CancellationToken cancellationToken) => service.AuditHistoryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get user audit history / 获取用户操作历史").WithTags("User");

            g.MapPost("DeviceList", (IUserService service, IHttpContextAccessor accessor, QueryIntRQ rq, CancellationToken cancellationToken) => service.DeviceListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get user device list / 获取用户设备列表").WithTags("User");

            g.MapGet("GetCurrentApps", (IUserService service, CancellationToken cancellationToken) => service.GetCurrentAppsAsync(cancellationToken))
                .WithDescription("Get user current applications / 获取用户当前程序").WithTags("User");

            g.MapGet("GetLatestApp", (IUserService service, CancellationToken cancellationToken) => service.GetLatestAppAsync(cancellationToken))
                .WithDescription("Get user's latest accessed appliation's Web URL / 获取用户最近访问的程序的Web网址").WithTags("User");

            g.MapPut("UpdateAvatar", (IUserService service, [FromForm] IFormFile avatar, CancellationToken cancellationToken) => service.UpdateAvatarAsync(avatar.OpenReadStream(), avatar.ContentType, cancellationToken))
                .DisableAntiforgery()
                .WithDescription("Update user avatar / 更新用户头像").WithTags("User");

            return builder;
        }
    }
}
