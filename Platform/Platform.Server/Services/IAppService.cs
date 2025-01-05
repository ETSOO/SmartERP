using com.etsoo.Utils.Actions;
using Platform.Server.Endpoints.App.RQ;
using System.Buffers;

namespace Platform.Server.Services
{
    public interface IAppService
    {
        Task<IActionResult> BuyAsync(AppBuyRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> BuyNewAsync(AppBuyNewRQ rq, CancellationToken cancellationToken = default);
        Task GetMyAsync(AppGetMyRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ListAsync(AppListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(AppQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryPurchasedAsync(AppPurchasedQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> RenewAsync(AppRenewRQ rq, CancellationToken cancellationToken = default);
    }
}