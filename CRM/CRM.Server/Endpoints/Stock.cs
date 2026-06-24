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

            g.MapPost("CreateLine", (IStockService service, StockCreateLineRQ rq, CancellationToken cancellationToken) => service.CreateLineAsync(rq, cancellationToken))
                .WithDescription("Create stock line, only for order & PO / 创建库存行，仅限订单和采购").WithTags("Stock");

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

            g.MapPost("QueryLines", (IStockService service, StockQueryLineRQ rq, CancellationToken cancellationToken) => service.QueryLinesAsync(rq, cancellationToken))
                .WithDescription("Query stock lines / 查询库存明细").WithTags("Stock");

            g.MapPost("QueryOrderLines", (IStockService service, StockQueryOrderLineRQ rq, CancellationToken cancellationToken) => service.QueryOrderLinesAsync(rq, cancellationToken))
                .WithDescription("Query order line items / 查询订单行项目").WithTags("Stock");

            g.MapPost("QueryProductLines", (IStockService service, StockQueryProductLineRQ rq, CancellationToken cancellationToken) => service.QueryProductLinesAsync(rq, cancellationToken))
                .WithDescription("Query stock product lines / 查询库存产品明细").WithTags("Stock");

            g.MapPost("QueryProduct", (IStockService service, StockQueryProductRQ rq, CancellationToken cancellationToken) => service.QueryProductAsync(rq, cancellationToken))
                .WithDescription("Query stock product / 查询库存产品").WithTags("Stock");

            g.MapGet("Read/{id:long}", (IStockService service, long id, CancellationToken cancellationToken) => service.ReadAsync(id, cancellationToken))
                .WithDescription("Read stock data for view / 读取用于浏览的库存数据").WithTags("Stock");

            g.MapGet("ReadLine/{id:long}", (IStockService service, long id, CancellationToken cancellationToken) => service.ReadLineAsync(id, true, cancellationToken))
                .WithDescription("Read stock line data / 读取库存行数据").WithTags("Stock");

            g.MapPost("Receive", (IStockService service, StockReceiveRQ rq, CancellationToken cancellationToken) => service.ReceiveAsync(rq, cancellationToken))
                .WithDescription("Receive stock / 入库").WithTags("Stock");

            g.MapGet("ReportAction", (IStockService service, CancellationToken cancellationToken) => service.ReportActionAsync(cancellationToken))
                .WithDescription("Get stock report action / 获取库存报表操作").WithTags("Stock");

            g.MapPost("Take", (IStockService service, StockTakeRQ rq, CancellationToken cancellationToken) => service.TakeAsync(rq, cancellationToken))
                .WithDescription("Stock take / 库存盘点").WithTags("Stock");

            g.MapPost("Transfer", (IStockService service, StockTransferRQ rq, CancellationToken cancellationToken) => service.TransferAsync(rq, cancellationToken))
                .WithDescription("Transfer stock / 调货").WithTags("Stock");

            g.MapPut("Update", (IStockService service, StockUpdateRQ rq, CancellationToken cancellationToken) => service.UpdateAsync(rq, cancellationToken))
                .WithDescription("Update stock / 更新库存").WithTags("Stock");

            g.MapPut("UpdateLine", (IStockService service, StockUpdateLineRQ rq, CancellationToken cancellationToken) => service.UpdateLineAsync(rq, cancellationToken))
                .WithDescription("Update stock line / 更新库存行").WithTags("Stock");

            return builder;
        }
    }
}
