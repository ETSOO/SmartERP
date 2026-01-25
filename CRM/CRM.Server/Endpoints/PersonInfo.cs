using com.etsoo.WebUtils;
using CRM.Server.RQ.PersonInfo;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Person info service APIs
    /// 人员信息服务接口
    /// </summary>
    internal static class PersonInfo
    {
        public static RouteGroupBuilder MapPersonInfo(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("PersonInfo");

            g.MapPost("Create", (IPersonInfoService service, PersonInfoCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create person info / 创建人员信息").WithTags("PersonInfo");

            g.MapDelete("Delete/{id:int}", (IPersonInfoService service, int id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete person info / 删除人员信息").WithTags("PersonInfo");

            g.MapPost("Query", (IPersonInfoService service, IHttpContextAccessor accessor, PersonInfoQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Person info query / 人员信息查询").WithTags("PersonInfo");

            g.MapGet("Read/{id:int}", (IPersonInfoService service, int id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read person info / 读取人员信息").WithTags("PersonInfo");

            g.MapPut("Update", (IPersonInfoService service, PersonInfoUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update person info / 更新人员信息").WithTags("PersonInfo");

            return builder;
        }
    }
}
