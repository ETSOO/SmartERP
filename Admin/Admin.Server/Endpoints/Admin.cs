using Admin.Server.RQ.Admin;
using Admin.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Server.Endpoints
{
    /// <summary>
    /// Admin service APIs
    /// 管理服务接口
    /// </summary>
    public static class Admin
    {
        public static RouteGroupBuilder MapAdmin(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Admin");

            g.MapPost("AppRenew", (IAdminService service, AppRenewRQ rq, CancellationToken cancellationToken) => service.AppRenewAsync(rq, cancellationToken))
                .WithDescription("App Renew / 应用续费").WithTags("Operation");

            g.MapPost("ClearUserFrozen/{userId:int}", (IAdminService service, [FromRoute] int userId, CancellationToken cancellationToken) => service.ClearUserFrozenAsync(userId, cancellationToken))
                .WithDescription("Clear User Frozen / 清除用户冻结").WithTags("Operation");

            return builder;
        }
    }
}
