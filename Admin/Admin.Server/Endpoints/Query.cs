using Admin.Server.RQ.Query;
using Admin.Server.Services;
using com.etsoo.WebUtils;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Server.Endpoints
{
    /// <summary>
    /// Query service APIs
    /// 查询服务API
    /// </summary>
    public static class Query
    {
        public static RouteGroupBuilder MapQuery(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Query");

            g.MapPost("AllApps", (IQueryService service, IHttpContextAccessor accessor, AllAppRQ rq, CancellationToken cancellationToken) => service.AllAppAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query all apps / 查询所有应用").WithTags("Query");

            g.MapPost("AllOrgs", (IQueryService service, IHttpContextAccessor accessor, AllOrgRQ rq, CancellationToken cancellationToken) => service.AllOrgAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query all organizations / 查询所有机构").WithTags("Query");

            g.MapPost("AllUsers", (IQueryService service, IHttpContextAccessor accessor, AllUserRQ rq, CancellationToken cancellationToken) => service.AllUserAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query all users / 查询所有用户").WithTags("Query");

            g.MapPost("AppList", (IQueryService service, IHttpContextAccessor accessor, AppListRQ rq, CancellationToken cancellationToken) => service.AppListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("App list / 应用列表").WithTags("Query");

            g.MapPost("AuditHistory", (IQueryService service, IHttpContextAccessor accessor, AuditHistoryRQ rq, CancellationToken cancellationToken) => service.AuditHistoryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query audit history / 查询操作历史").WithTags("Query");

            g.MapPost("OrgList", (IQueryService service, IHttpContextAccessor accessor, OrgListRQ rq, CancellationToken cancellationToken) => service.OrgListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Organization list / 机构列表").WithTags("Query");

            g.MapGet("ReadApp/{id:int}", (IQueryService service, IHttpContextAccessor accessor, [FromRoute] int id, CancellationToken cancellationToken) => service.ReadAppAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read app data / 读取应用数据").WithTags("Query");

            g.MapGet("ReadOrg/{id:int}", (IQueryService service, IHttpContextAccessor accessor, [FromRoute] int id, CancellationToken cancellationToken) => service.ReadOrgAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read organization data / 读取机构数据").WithTags("Query");

            g.MapGet("ReadUser/{id:int}", (IQueryService service, IHttpContextAccessor accessor, [FromRoute] int id, CancellationToken cancellationToken) => service.ReadUserAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read user data / 读取用户数据").WithTags("Query");

            g.MapPost("UserList", (IQueryService service, IHttpContextAccessor accessor, UserListRQ rq, CancellationToken cancellationToken) => service.UserListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("User list / 用户列表").WithTags("Query");

            return builder;
        }
    }
}
