using com.etsoo.Utils.Actions;
using Platform.Server.Dto.Org;
using Platform.Server.Endpoints.Org.RQ;
using System.Buffers;

namespace Platform.Server.Services
{
    public interface IOrgService
    {
        Task<IActionResult> CreateAsync(OrgCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrgQueryData>> QueryAsync(OrgQueryRQ rq, CancellationToken cancellationToken = default);
        Task QueryJsonAsync(OrgQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(OrgUpdateRQ rq, CancellationToken cancellationToken = default);
    }
}