using com.etsoo.Utils.Actions;
using CRM.Server.Dto.POLine;
using CRM.Server.RQ.POLine;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IPOLineService
    {
        Task<IActionResult> CompleteAsync(POLineCompleteRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> CreateAsync(POLineCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task ListAsync(POLineListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<POLineQueryAllData[]> QueryAllAsync(POLineQueryAllRQ rq, CancellationToken cancellationToken = default);
        Task<POLineQueryData[]> QueryAsync(POLineQueryRQ rq, CancellationToken cancellationToken = default);
        Task<POLineViewData?> ReadAsync(long id, CancellationToken cancellationToken = default);
        Task<IActionResult> RollbackAsync(long id, CancellationToken cancellationToken = default);
        Task<IActionResult> StartAsync(POLineStartRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(POLineUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<POLineUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default);
    }
}