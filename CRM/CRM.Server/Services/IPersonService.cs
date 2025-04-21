using CRM.Server.Dto.Person;
using CRM.Server.RQ.Person;
using PlatformShared.Dto;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IPersonService
    {
        Task<ChoosePersonsData> ChoosePersonsAsync(ChoosePersonsRQ rq, CancellationToken cancellationToken = default);
        Task<IEnumerable<ContactItem>> ListAsync(PersonListRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(PersonListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(PersonQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<PersonViewData?> ReadAsync(long id, CancellationToken cancellationToken = default);
    }
}