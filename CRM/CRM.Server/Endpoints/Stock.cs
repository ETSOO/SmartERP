using com.etsoo.WebUtils;
using CRM.Server.RQ.Stock;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    internal static class Stock
    {
        public static RouteGroupBuilder MapStock(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("Stock");

            g.MapPost("Assemble", (IStockService service, StockAssembleRQ rq, CancellationToken cancellationToken) => service.AssembleAsync(rq, cancellationToken))
                .WithDescription("Assemble stock / 组装库存").WithTags("Stock");

            g.MapPost("Check", (IStockService service, CheckStockRQ rq, CancellationToken cancellationToken) => service.CheckStockAsync(rq, cancellationToken))
                .WithDescription("Check stock / 检查库存").WithTags("Stock");

            g.MapDelete("Delete/{id:long}", (IStockService service, long id, CancellationToken cancellationToken) => service.DeleteAsync(id, cancellationToken))
                .WithDescription("Delete stock / 删除库存").WithTags("Stock");

            g.MapPost("Lose", (IStockService service, StockLoseRQ rq, CancellationToken cancellationToken) => service.LoseAsync(rq, cancellationToken))
                .WithDescription("Stock loss / 库存报损").WithTags("Stock");

            g.MapPost("Init", (IStockService service, StockInitRQ rq, CancellationToken cancellationToken) => service.InitAsync(rq, cancellationToken))
                .WithDescription("Init stock / 初始化库存").WithTags("Stock");

            g.MapPost("List", (IStockService service, StockListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get stock list / 获取库存列表").WithTags("Stock");

            g.MapPost("OrderOut", (IStockService service, StockOrderOutRQ rq, CancellationToken cancellationToken) => service.OrderOutAsync(rq, cancellationToken))
                .WithDescription("Order delivering / 订单发货").WithTags("Stock");

            g.MapPost("POIn", (IStockService service, StockPOInRQ rq, CancellationToken cancellationToken) => service.POInAsync(rq, cancellationToken))
                .WithDescription("PO receiving / 采购入库").WithTags("Stock");

            g.MapPost("Query", (IStockService service, StockQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query stock info / 查询库存信息").WithTags("Stock");

            g.MapPost("Receive", (IStockService service, StockReceiveRQ rq, CancellationToken cancellationToken) => service.ReceiveAsync(rq, cancellationToken))
                .WithDescription("Receive stock / 入库").WithTags("Stock");

            g.MapPost("Transfer", (IStockService service, StockTransferRQ rq, CancellationToken cancellationToken) => service.TransferAsync(rq, cancellationToken))
                .WithDescription("Transfer stock / 调货").WithTags("Stock");

            return builder;
        }
    }
}
