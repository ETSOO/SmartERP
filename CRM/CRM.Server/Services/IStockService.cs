using CRM.Server.Dto.Stock;
using CRM.Server.RQ.Stock;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IStockService
    {
        Task ListAsync(StockListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<StockQueryData[]> QueryAsync(StockQueryRQ rq, CancellationToken cancellationToken = default);
    }
}