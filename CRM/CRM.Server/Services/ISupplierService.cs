using CRM.Server.RQ.Supplier;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface ISupplierService
    {
        Task ListAsync(SupplierListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(SupplierQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}