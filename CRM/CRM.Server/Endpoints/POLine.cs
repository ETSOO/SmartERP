using com.etsoo.WebUtils;
using CRM.Server.RQ.POLine;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Purchase line service APIs
    /// 采购行服务API
    /// </summary>
    internal static class POLine
    {
        public static RouteGroupBuilder MapPOLine(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("POLine");

            g.MapPut("Complete", (IPOLineService service, POLineCompleteRQ rq, CancellationToken cancellationToken) => service.CompleteAsync(rq, cancellationToken))
                .WithDescription("Complete purchase line / 完成采购行").WithTags("POLine");

            g.MapPut("Create", (IPOLineService service, POLineCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create purchase line / 创建采购行").WithTags("POLine");

            g.MapDelete("Delete/{id:long}", (IPOLineService service, long id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete purchase line / 删除采购行").WithTags("POLine");

            g.MapPost("List", (IPOLineService service, POLineListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get purchase line list / 获取采购行列表").WithTags("POLine");

            g.MapPost("Query", (IPOLineService service, POLineQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query purchase line info / 查询采购行信息").WithTags("POLine");

            g.MapPost("QueryAll", (IPOLineService service, POLineQueryAllRQ rq, CancellationToken cancellationToken) => service.QueryAllAsync(rq, cancellationToken))
                .WithDescription("Query all purchase lines / 查询所有采购行").WithTags("POLine");

            g.MapGet("Read/{id:long}", (IPOLineService service, long id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read purchase line info / 读取采购行信息").WithTags("POLine");

            g.MapPut("Rollback/{id:long}", (IPOLineService service, long id, CancellationToken cancellationToken) => service.RollbackAsync(id, cancellationToken))
                .WithDescription("Rollback purchase line / 回滚采购行").WithTags("POLine");

            g.MapPut("Start", (IPOLineService service, POLineStartRQ rq, CancellationToken cancellationToken) => service.StartAsync(rq, cancellationToken))
                .WithDescription("Start to execute purchase line / 开始执行采购行").WithTags("POLine");

            g.MapPut("Update", (IPOLineService service, POLineUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update purchase line / 更新采购行").WithTags("POLine");

            g.MapGet("UpdateRead/{id:long}", (IPOLineService service, long id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Read purchase line data for update / 读取用于更新的采购行数据").WithTags("POLine");

            return builder;
        }
    }
}
