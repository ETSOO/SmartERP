using CRM.Server.RQ.Group;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IGroupService
    {
        Task ListAsync(GroupListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(GroupQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}