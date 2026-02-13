using com.etsoo.Utils.Actions;
using CRM.Server.Dto.PersonProduct;
using CRM.Server.RQ.PersonProduct;

namespace CRM.Server.Services
{
    public interface IPersonProductService
    {
        Task<IActionResult> CreateAsync(PersonProductCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(long personId, int productId, CancellationToken cancellationToken = default);
        Task<PersonProductQueryData[]> QueryAsync(PersonProductQueryRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(PersonProductUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<PersonProductUpdateReadData?> UpdateReadAsync(long personId, int productId, CancellationToken cancellationToken = default);
    }
}