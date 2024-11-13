using com.etsoo.CoreFramework.DB;
using Platform.Server.Endpoints.Org.RQ;
using Platform.Server.Services;

namespace Platform.Server.Endpoints.Org
{
    /// <summary>
    /// Organization service APIs
    /// 机构服务API
    /// </summary>
    public static class Org
    {
        public static RouteGroupBuilder MapOrg(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Org");

            g.MapPut("Create", (IOrgService service, OrgCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create organization / 创建新机构").WithTags("Org");

            g.MapDelete("Delete/{id:int}", (IOrgService service, int id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete organization / 删除机构").WithTags("Org");

            g.MapPost("Query", (IOrgService service, OrgQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query organizations / 查询机构").WithTags("Org");

            g.MapPost("QueryJson", async (IOrgService service, OrgQueryRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) =>
            {
                var response = accessor.HttpContext?.Response;
                if (response == null)
                {
                    return;
                }
                response.JsonContentType();
                await service.QueryJsonAsync(rq, response.BodyWriter, cancellationToken);
            }).WithDescription("Query organizations JSON data / 查询机构JSON数据").WithTags("Org");

            g.MapPut("Update", (IOrgService service, OrgUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update organization / 更新机构").WithTags("Org");

            return builder;
        }
    }
}
