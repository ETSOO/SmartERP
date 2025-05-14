using CRM.Server.RQ.Customer;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface ICustomerService
    {
        Task ListAsync(CustomerListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(CustomerQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}