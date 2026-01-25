using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Asset;
using CRM.Server.RQ.Asset;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IAssetService
    {
        Task<IActionResult> CreateAsync(AssetCreateRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(AssetListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<AssetQueryData[]> QueryAsync(AssetQueryRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(AssetUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<AssetUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default);
    }
}