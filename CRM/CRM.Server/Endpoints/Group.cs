using com.etsoo.WebUtils;
using CRM.Server.RQ.Group;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Group service APIs
    /// 用户服务API
    /// </summary>
    internal static class Group
    {
        public static RouteGroupBuilder MapGroup(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Group");

            g.MapPost("List", (IGroupService service, GroupListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get permission group list / 获取权限组列表").WithTags("Group");

            g.MapPost("Query", (IGroupService service, GroupQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query permission group info / 查询权限组信息").WithTags("Group");

            return builder;
        }
    }
}
