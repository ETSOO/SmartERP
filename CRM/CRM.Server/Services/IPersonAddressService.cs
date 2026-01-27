using com.etsoo.Utils.Actions;
using CRM.Server.Dto.PersonAddress;
using CRM.Server.RQ.PersonAddress;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IPersonAddressService
    {
        Task<IActionResult> CreateAsync(AddressCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> CreateLocationAsync(AddressLocationCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task ListAsync(AddressListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<AddressQueryData[]> QueryAsync(AddressListRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(AddressUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<AddressUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default);
    }
}