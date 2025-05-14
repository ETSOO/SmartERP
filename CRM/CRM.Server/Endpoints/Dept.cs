using com.etsoo.WebUtils;
using CRM.Server.RQ.Dept;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Department service APIs
    /// 部门服务API
    /// </summary>
    internal static class Dept
    {
        public static RouteGroupBuilder MapDept(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Dept");

            g.MapPost("List", (IDeptService service, DeptListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get department list / 获取部门列表").WithTags("Dept");

            g.MapPost("Query", (IDeptService service, DeptQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query department info / 查询部门信息").WithTags("Dept");

            return builder;
        }
    }
}
