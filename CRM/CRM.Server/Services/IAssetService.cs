using CRM.Server.RQ.Asset;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IAssetService
    {
        Task ListAsync(AssetListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(AssetQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}