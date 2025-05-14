using CRM.Server.RQ.Order;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IOrderService
    {
        Task ListAsync(OrderListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(OrderQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}