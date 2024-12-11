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
        Task GetMyAsync(OrgGetMyRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ListAsync(OrgListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrgQueryData>> QueryAsync(OrgQueryRQ rq, CancellationToken cancellationToken = default);
        Task QueryAsync(OrgQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(OrgUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAvatarAsync(int id, Stream avatarStream, string contentType, CancellationToken cancellationToken = default);
        Task UpdateReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}