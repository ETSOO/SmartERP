using com.etsoo.Utils.Actions;
using CRM.Server.Dto.FinanceAccount;
using CRM.Server.RQ.FinanceAccount;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IFinanceAccountService
    {
        Task<IActionResult> CreateAsync(FinanceAccountCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> CreateBulkAsync(FinanceAccountCreateBulkRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> CreateCashAsync(FinanceAccountCreateCashRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<FinanceAccountListData[]?> ListAsync(FinanceAccountListRQ rq, CancellationToken cancellationToken = default);
        Task<FinanceAccountQueryData[]?> QueryAsync(FinanceAccountQueryRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(FinanceAccountUpdateRQ rq, CancellationToken cancellationToken = default);
        Task UpdateReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}