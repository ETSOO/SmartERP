using com.etsoo.Utils.Actions;
using CRM.Server.Dto.PO;
using CRM.Server.RQ.PO;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IPOService
    {
        Task<(bool IsEdit, bool IsManage)> CheckEditPermissionsAsync(CancellationToken cancellationToken = default);
        Task<IActionResult> CreateAsync(POCreateRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(POListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<POQueryData[]> QueryAsync(POQueryRQ rq, CancellationToken cancellationToken = default);
        Task<POViewData?> ReadAsync(long id, CancellationToken cancellationToken = default);
        Task<IActionResult> RecalculateAsync(long id, bool checkPermission, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(POUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<POUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default);
    }
}
