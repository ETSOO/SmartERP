using com.etsoo.Utils.Actions;
using CRM.Server.RQ.PersonContact;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IPersonContactService
    {
        Task<IActionResult> AddAsync(ContactRelationAddRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> CreateAsync(ContactCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task ListAsync(ContactListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(ContactQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateRelationAsync(ContactRelationUpdateRQ rq, CancellationToken cancellationToken = default);
        Task UpdateRelationReadAsync(long id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}