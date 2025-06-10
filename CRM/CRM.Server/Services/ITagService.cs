using CRM.Server.RQ.Tag;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface ITagService
    {
        Task<string[]> ListAsync(TagListRQ rq, CancellationToken cancellationToken = default);
        Task QueryAsync(TagQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}