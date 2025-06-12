using com.etsoo.Utils.Actions;
using Platform.Server.Endpoints.Member.RQ;
using System.Buffers;

namespace Platform.Server.Services
{
    public interface IMemberService
    {
        Task<IActionResult> AdjustReportToAsync(MemberAdjustReportToRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task ListAsync(MemberListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> InviteAsync(MemberInviteRQ rq);
        Task QueryAsync(MemberQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ReadAsync(long id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(MemberUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAvatarAsync(long id, Stream avatarStream, string contentType, CancellationToken cancellationToken = default);
        Task UpdateReadAsync(long id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}