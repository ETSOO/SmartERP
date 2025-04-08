using com.etsoo.Utils.Actions;
using CRM.Server.Dto.PersonProfile;
using CRM.Server.RQ.PersonProfile;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IPersonProfileService
    {
        Task<IActionResult> CreateAsync(PersonProfileCreateRQ rq, string? indexKey = null, CancellationToken cancellationToken = default);
        Task<IActionResult> CreateTaskAsync(PersonTaskCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAttachmentAsync(long id, CancellationToken cancellationToken = default);
        Task ListAsync(PersonProfileListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(PersonProfileQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<PersonProfileViewData?> ReadAsync(long id, CancellationToken cancellationToken = default);
        Task<PersonProfileInnerViewData?> ReadInnerAsync(long id, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(PersonProfileUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAttachmentAsync(PersonProfileAttachmentUpdateRQ rq, CancellationToken cancellationToken = default);
        Task UpdateReadAsync(long id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}