using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.PersonCategory;
using CRM.Server.RQ.PersonCategory;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IPersonCategoryService
    {
        Task<IActionResult> CreateAsync(PersonCategoryCreateRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(PersonCategoryListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> MergeAsync(MergeRQ rq, CancellationToken cancellationToken = default);
        Task<PersonCategoryQueryData[]> QueryAsync(PersonCategoryQueryRQ rq, CancellationToken cancellationToken = default);
        Task<int> SortAsync(Dictionary<int, short> rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(PersonCategoryUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<PersonCategoryUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default);
    }
}