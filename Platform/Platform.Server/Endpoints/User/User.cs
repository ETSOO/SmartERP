using com.etsoo.WebUtils;
using Microsoft.AspNetCore.Mvc;
using Platform.Server.Endpoints.AuthCode.RQ;
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

            g.MapPost("AddEmail", (IUserService service, CodeValidateRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.AddEmailAsync(rq, accessor.UserAgent(), cancellationToken))
                .WithDescription("Add user email / 添加用户邮箱").WithTags("User");

            g.MapPost("AddMobile", (IUserService service, CodeValidateRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.AddMobileAsync(rq, accessor.UserAgent(), cancellationToken))
                .WithDescription("Add user mobile / 添加用户手机").WithTags("User");

            g.MapPost("AllIdentifiers", (IUserService service, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.AllIdentifiersAsync(accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get all user identifiers / 获取所有用户标识").WithTags("User");

            g.MapPost("AuditHistory", (IUserService service, IHttpContextAccessor accessor, AuditHistoryRQ rq, CancellationToken cancellationToken) => service.AuditHistoryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get user audit history / 获取用户操作历史").WithTags("User");

            g.MapPost("CheckSession/{id:int}", (IUserService service, int id, CancellationToken cancellationToken) => service.CheckSessionAsync(id, cancellationToken))
                .WithDescription("Check app session / 检查应用会话").WithTags("User");

            g.MapDelete("DeleteIdentifier/{id:int}", (IUserService service, int id, CancellationToken cancellationToken) => service.DeleteIdentifierAsync(id, cancellationToken))
                .WithDescription("Delete user identifier / 删除用户标识").WithTags("User");

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
