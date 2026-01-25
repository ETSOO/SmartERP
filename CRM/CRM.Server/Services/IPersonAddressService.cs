using com.etsoo.Utils.Actions;
using CRM.Server.Dto.PersonAddress;
using CRM.Server.RQ.PersonAddress;

namespace CRM.Server.Services
{
    public interface IPersonAddressService
    {
        Task<IActionResult> CreateAsync(AddressCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(AddressUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<AddressUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default);
    }
}