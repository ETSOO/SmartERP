using com.etsoo.Utils.Actions;
using CRM.Server.Dto.FinanceTransaction;
using CRM.Server.RQ.FinanceTransaction;

namespace CRM.Server.Services
{
    public interface IFinanceTransactionService
    {
        Task<IActionResult> AdjustAsync(FinanceTransactionAdjustRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> OffsetAsync(FinanceTransactionOffsetRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> PayOrderAsync(FinanceTransactionPayOrderRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> PayPOAsync(FinanceTransactionPayPORQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> ReimburseAsync(FinanceTransactionReimburseRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> TransferAsync(FinanceTransactionTransferRQ rq, CancellationToken cancellationToken = default);
        Task<FinanceTransactionViewOrderData?> ViewOrderAsync(long id, CancellationToken cancellationToken);
        Task<FinanceTransactionViewPOData?> ViewPOAsync(long id, CancellationToken cancellationToken);
    }
}