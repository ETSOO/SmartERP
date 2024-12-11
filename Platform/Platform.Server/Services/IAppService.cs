using Platform.Server.Endpoints.App.RQ;
using System.Buffers;

namespace Platform.Server.Services
{
    public interface IAppService
    {
        Task GetMyAsync(AppGetMyRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ListAsync(AppListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(AppQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryPurchasedAsync(AppPurchasedQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}