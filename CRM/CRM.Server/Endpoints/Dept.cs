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

            g.MapPost("Create", (IDeptService service, DeptCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create department / 创建部门").WithTags("Dept");

            g.MapPost("List", (IDeptService service, DeptListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get department list / 获取部门列表").WithTags("Dept");

            g.MapPost("Query", (IDeptService service, DeptQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Query department info / 查询部门信息").WithTags("Dept");

            g.MapPut("Update", (IDeptService service, DeptUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update department / 更新部门").WithTags("Dept");

            g.MapGet("UpdateRead/{id:long}", (IDeptService service, long id, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.UpdateReadAsync(id, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Read department data for update / 读取用于更新的部门数据").WithTags("Dept");

            return builder;
        }
    }
}
