using com.etsoo.WebUtils;
using CRM.Server.RQ.OrderLine;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    /// <summary>
    /// Order line service APIs
    /// 订单行服务API
    /// </summary>
    internal static class OrderLine
    {
        public static RouteGroupBuilder MapOrderLine(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("OrderLine");

            g.MapPut("Complete", (IOrderLineService service, OrderLineCompleteRQ rq, CancellationToken cancellationToken) => service.CompleteAsync(rq, cancellationToken))
                .WithDescription("Complete order line / 完成订单行").WithTags("OrderLine");

            g.MapPost("Create", (IOrderLineService service, OrderLineCreateRQ rq, CancellationToken cancellationToken) => service.CreateAsync(rq, cancellationToken))
                .WithDescription("Create order line / 创建订单行").WithTags("OrderLine");

            g.MapDelete("Delete/{id:long}", (IOrderLineService service, long id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete order line / 删除订单行").WithTags("OrderLine");

            g.MapPost("List", (IOrderLineService service, OrderLineListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get order line list / 获取订单行列表").WithTags("OrderLine");

            g.MapPost("Query", (IOrderLineService service, OrderLineQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query order line info / 查询订单行信息").WithTags("OrderLine");

            g.MapPost("QueryAll", (IOrderLineService service, OrderLineQueryAllRQ rq, CancellationToken cancellationToken) => service.QueryAllAsync(rq, cancellationToken))
                .WithDescription("Query order line info / 查询订单行信息").WithTags("OrderLine");

            g.MapPost("QueryAsset", (IOrderLineService service, OrderLineQueryAssetRQ rq, CancellationToken cancellationToken) => service.QueryAssetAsync(rq, cancellationToken))
                .WithDescription("Query order line asset info / 查询订单行资产信息").WithTags("OrderLine");

            g.MapGet("Read/{id:long}", (IOrderLineService service, long id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read order line info / 读取订单行信息").WithTags("OrderLine");

            g.MapPut("Rollback/{id:long}", (IOrderLineService service, long id, CancellationToken cancellationToken) => service.RollbackAsync(id, cancellationToken))
                .WithDescription("Rollback order line / 回滚订单行").WithTags("OrderLine");

            g.MapPut("Start", (IOrderLineService service, OrderLineStartRQ rq, CancellationToken cancellationToken) => service.StartAsync(rq, cancellationToken))
                .WithDescription("Start to execute order line / 开始执行订单行").WithTags("OrderLine");

            g.MapPut("Update", (IOrderLineService service, OrderLineUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update order line / 更新订单行").WithTags("OrderLine");

            g.MapGet("UpdateRead/{id:long}", (IOrderLineService service, long id, CancellationToken cancellationToken) => service.UpdateReadAsync(id, cancellationToken))
                .WithDescription("Read order line data for update / 读取用于更新的订单行数据").WithTags("OrderLine");

            return builder;
        }
    }
}
