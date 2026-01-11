using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Product;
using CRM.Server.RQ.Product;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IProductService
    {
        Task<IActionResult> CreateAsync(ProductCreateRQ rq, CancellationToken cancellationToken = default);
        ValueTask<ProductDuplicateTestData[]?> DuplicateTestAsync(ProductDuplicateTestRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(ProductListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<ProductQueryData[]> QueryAsync(ProductQueryRQ rq, CancellationToken cancellationToken = default);
        Task<ProductUnitItem[]> QueryUnitAsync(CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(ProductUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<ProductUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default);
        Task<int> UpdateUnitAsync(ProductUnitUpdateRQ rq, CancellationToken cancellationToken = default);
    }
}