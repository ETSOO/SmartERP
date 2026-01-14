using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.ProductCategory;
using CRM.Server.RQ.ProductCategory;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IProductCategoryService
    {
        Task<IActionResult> CreateAsync(ProductCategoryCreateRQ rq, CancellationToken cancellationToken = default);
        ValueTask<ProductCategoryDuplicateTestData[]?> DuplicateTestAsync(ProductCategoryDuplicateTestRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(ProductCategoryListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> MergeAsync(MergeRQ rq, CancellationToken cancellationToken = default);
        Task<ProductCategoryQueryData[]> QueryAsync(ProductCategoryQueryRQ rq, CancellationToken cancellationToken = default);
        Task<int> SortAsync(Dictionary<int, short> rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(ProductCategoryUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<ProductCategoryUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default);
    }
}
