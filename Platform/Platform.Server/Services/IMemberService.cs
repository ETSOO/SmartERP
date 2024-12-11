using com.etsoo.Utils.Actions;
using Platform.Server.Endpoints.Member.RQ;
using System.Buffers;

namespace Platform.Server.Services
{
    public interface IMemberService
    {
        Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task ListAsync(MemberListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(MemberQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}