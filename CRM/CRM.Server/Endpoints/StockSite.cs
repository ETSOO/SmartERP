using CRM.Server.RQ.StockSite;
using CRM.Server.Services;

namespace CRM.Server.Endpoints
{
    internal static class StockSite
    {
        public static RouteGroupBuilder MapStockSite(this RouteGroupBuilder builder)
        {
            var g = builder.MapGroup("StockSite");

            g.MapPost("Query", (IStockSiteService service, StockSiteQueryRQ rq, CancellationToken cancellationToken) => service.QueryAsync(rq, cancellationToken))
                .WithDescription("Query stock site info / 查询库存点信息").WithTags("StockSite");

            g.MapGet("ViewProduct/{id:int}", (IStockSiteService service, int id, CancellationToken cancellationToken) => service.ViewProductAsync(id, cancellationToken))
                .WithDescription("View product stocks / 浏览产品库存").WithTags("StockSite");

            return builder;
        }
    }
}
