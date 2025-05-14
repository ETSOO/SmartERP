using CRM.Server.RQ.PO;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IPOService
    {
        Task ListAsync(POListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(POQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}