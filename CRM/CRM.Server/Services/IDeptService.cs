using CRM.Server.RQ.Dept;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IDeptService
    {
        Task ListAsync(DeptListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(DeptQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}