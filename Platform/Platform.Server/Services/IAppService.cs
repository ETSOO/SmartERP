using com.etsoo.Utils.Actions;
using Platform.Server.Dto.App;
using Platform.Server.Endpoints.App.RQ;
using System.Buffers;

namespace Platform.Server.Services
{
    public interface IAppService : ICommonService
    {
        Task<IActionResult> BuyAsync(AppBuyRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> BuyNewAsync(AppBuyNewRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> CreateApiKeyAsync(int id, string passphase, CancellationToken cancellationToken = default);
        Task<IEnumerable<AppData>> GetMyAsync(AppGetMyRQ rq, CancellationToken cancellationToken = default);
        Task GetMyAsync(AppGetMyRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ListAsync(AppListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(AppQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryPurchasedAsync(AppPurchasedQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> RenewAsync(AppRenewRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(AppUpdateRQ rq, CancellationToken cancellationToken = default);
        Task UpdateReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}