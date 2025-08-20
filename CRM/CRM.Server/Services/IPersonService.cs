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
        Task<IActionResult> CreateAddressAsync(AddressCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> CreateContactAsync(ContactCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> CreateInfoAsync(PersonInfoCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAddressAsync(int id, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteInfoAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ContactItem>> ListAsync(PersonListRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(PersonListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ListContactAsync(ContactListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(PersonQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryContactAsync(ContactQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryInfoAsync(PersonInfoQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<PersonViewData?> ReadAsync(long id, CancellationToken cancellationToken = default);
        Task<string?> ReadInfoAsync(int id, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(PersonUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAddressAsync(AddressUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateContactRelationAsync(ContactRelationUpdateRQ rq, CancellationToken cancellationToken = default);
        Task UpdateContactRelationReadAsync(long personId, long contactId, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateInfoAsync(PersonInfoUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<AddressUpdateReadData?> UpdateAddressReadAsync(int id, CancellationToken cancellationToken = default);
        Task<PersonUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default);
    }
}