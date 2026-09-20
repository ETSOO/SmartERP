using com.etsoo.Utils.Actions;
using CRM.Server.RQ.FinanceTransaction;

namespace CRM.Server.Services
{
    public interface IFinanceTransactionService
    {
        Task<IActionResult> AdjustAsync(FinanceTransactionAdjustRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> OffsetAsync(FinanceTransactionOffsetRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> PayOrderAsync(FinanceTransactionPayOrderRQ rq, CancellationToken cancellationToken = default);
    }
}