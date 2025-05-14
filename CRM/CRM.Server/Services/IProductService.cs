using CRM.Server.RQ.Product;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IProductService
    {
        Task ListAsync(ProductListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(ProductQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}