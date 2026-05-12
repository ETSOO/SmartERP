using CRM.Server.Dto.StockSite;
using CRM.Server.RQ.StockSite;

namespace CRM.Server.Services
{
    public interface IStockSiteService
    {
        Task<StockSiteQueryData[]> QueryAsync(StockSiteQueryRQ rq, CancellationToken cancellationToken = default);
        Task<StockSiteViewProductData[]> ViewProductAsync(int id, CancellationToken cancellationToken = default);
    }
}