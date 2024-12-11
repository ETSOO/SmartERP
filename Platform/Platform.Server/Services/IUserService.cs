using com.etsoo.Utils.Actions;
using Platform.Server.Dto.App;
using Platform.Server.Endpoints.User.RQ;
using System.Buffers;

namespace Platform.Server.Services
{
    public interface IUserService
    {
        Task AuditHistoryAsync(AuditHistoryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task DeviceListAsync(QueryIntRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IEnumerable<AppData>> GetCurrentAppsAsync(CancellationToken cancellationToken = default);
        Task<string> GetLatestAppAsync(CancellationToken cancellationToken = default);
        ValueTask<IActionResult> UpdateAvatarAsync(Stream avatarStream, string contentType, CancellationToken cancellationToken = default);
    }
}