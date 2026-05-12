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

            g.MapPost("List", (IStockService service, StockListRQ rq, IHttpContextAccessor accessor, CancellationToken cancellationToken) => service.ListAsync(rq, accessor.GetJsonWriter(), cancellationToken))
                .WithDescription("Get stock list / 获取库存列表").WithTags("Stock");

            g.MapPost("Query", (IStockService service, StockQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query stock info / 查询库存信息").WithTags("Stock");

            return builder;
        }
    }
}
