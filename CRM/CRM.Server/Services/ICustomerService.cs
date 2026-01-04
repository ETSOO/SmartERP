using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Customer;
using CRM.Server.RQ.Customer;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface ICustomerService
    {
        Task<IActionResult> CreateAsync(CustomerCreateRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(CustomerListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<CustomerQueryData[]> QueryAsync(CustomerQueryRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(CustomerUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<CustomerUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default);
    }
}