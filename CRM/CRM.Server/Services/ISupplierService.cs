using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Supplier;
using CRM.Server.RQ.Supplier;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface ISupplierService
    {
        Task<IActionResult> CreateAsync(SupplierCreateRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(SupplierListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(SupplierQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(SupplierUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<SupplierUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default);
    }
}