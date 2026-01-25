using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Person;
using CRM.Server.RQ.Person;
using PlatformShared.Dto;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IPersonService
    {
        Task<ChoosePersonsData> ChoosePersonsAsync(ChoosePersonsRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken = default);
        ValueTask<PersonDuplicateTestData[]?> DuplicateTestAsync(PersonDuplicateTestRQ rq, CancellationToken cancellationToken = default);
        Task<bool> IsDeletableAsync(long id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ContactItem>> ListAsync(PersonListRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(PersonListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IEnumerable<PersonQueryData>> QueryAsync(PersonQueryRQ rq, CancellationToken cancellationToken = default);
        Task QueryAsync(PersonQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<PersonViewData?> ReadAsync(long id, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(PersonUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<PersonUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default);
    }
}