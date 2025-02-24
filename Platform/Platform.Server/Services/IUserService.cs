using com.etsoo.Utils.Actions;
using Platform.Server.Dto.App;
using Platform.Server.Dto.User;
using Platform.Server.Endpoints.AuthCode.RQ;
using Platform.Server.Endpoints.User.RQ;
using System.Buffers;

namespace Platform.Server.Services
{
    public interface IUserService
    {
        ValueTask<IActionResult> AddEmailAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> AddMobileAsync(CodeValidateRQ rq, string? userAgent, CancellationToken cancellationToken = default);
        Task<UserIdentifierData[]> AllIdentifiersAsync(CancellationToken cancellationToken = default);
        Task AllIdentifiersAsync(IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task AuditHistoryAsync(AuditHistoryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> CheckSessionAsync(int id, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteIdentifierAsync(int id, CancellationToken cancellationToken = default);
        Task DeviceListAsync(QueryIntRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IEnumerable<AppData>> GetCurrentAppsAsync(CancellationToken cancellationToken = default);
        Task<AppData> GetLatestAppAsync(CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(UserUpdateRQ rq, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> UpdateAvatarAsync(Stream avatarStream, string contentType, CancellationToken cancellationToken = default);
        Task UpdateReadAsync(IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}