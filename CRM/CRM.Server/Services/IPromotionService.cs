using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Promotion;
using CRM.Server.RQ.Promotion;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IPromotionService
    {
        Task<IActionResult> CreateAsync(PromotionCreateRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(PromotionListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<PromotionQueryData[]> QueryAsync(PromotionQueryRQ rq, CancellationToken cancellationToken = default);
        Task<int> SortAsync(Dictionary<int, short> rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(PromotionUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<PromotionUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default);
    }
}