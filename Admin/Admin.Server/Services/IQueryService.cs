using Admin.Server.RQ.Query;
using com.etsoo.ServiceApp.SmartERP;
using System.Buffers;

namespace Admin.Server.Services
{
    public interface IQueryService : ISEUserService
    {
        Task AllAppAsync(AllAppRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task AllOrgAsync(AllOrgRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task AllUserAsync(AllUserRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task AppListAsync(AppListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task AuditHistoryAsync(AuditHistoryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task OrgListAsync(OrgListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ReadAppAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ReadOrgAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ReadUserAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task UserListAsync(UserListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}