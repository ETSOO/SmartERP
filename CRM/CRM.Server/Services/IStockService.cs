using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Stock;
using CRM.Server.RQ.Stock;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IStockService
    {
        Task<IActionResult> AssembleAsync(StockAssembleRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> CheckStockAsync(CheckStockRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> CheckStockAsync(int locationId, StockItem[] items, CancellationToken cancellationToken = default);
        Task<IActionResult> CreateLineAsync(StockCreateLineRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task<IActionResult> LoseAsync(StockLoseRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> InitAsync(StockInitRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(StockListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> OrderOutAsync(StockOrderOutRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> POInAsync(StockPOInRQ rq, CancellationToken cancellationToken = default);
        Task<StockQueryData[]> QueryAsync(StockQueryRQ rq, CancellationToken cancellationToken = default);
        Task<StockQueryLineData[]> QueryLinesAsync(StockQueryLineRQ rq, CancellationToken cancellationToken);
        Task<StockQueryOrderLineData[]> QueryOrderLinesAsync(StockQueryOrderLineRQ rq, CancellationToken cancellationToken);
        Task<StockQueryProductLineData[]> QueryProductLinesAsync(StockQueryProductLineRQ rq, CancellationToken cancellationToken);
        Task<StockQueryProductData[]> QueryProductAsync(StockQueryProductRQ rq, CancellationToken cancellationToken = default);
        Task<StockViewData?> ReadAsync(long id, CancellationToken cancellationToken = default);
        Task<StockLineViewData?> ReadLineAsync(long id, bool checkPermission, CancellationToken cancellationToken = default);
        Task<IActionResult> ReceiveAsync(StockReceiveRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> TakeAsync(StockTakeRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> TransferAsync(StockTransferRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(StockUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateLineAsync(StockUpdateLineRQ rq, CancellationToken cancellationToken = default);
    }
}