using CRM.Server.Dto.Group;
using CRM.Server.RQ.Group;
using PlatformShared.Dto;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IGroupService
    {
        Task ListAsync(GroupListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(GroupQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryItemsAsync(IBufferWriter<byte> writer, AppModule? module = null, CancellationToken cancellationToken = default);
        Task<GroupViewData?> ReadAsync(int id, CancellationToken cancellationToken = default);
    }
}