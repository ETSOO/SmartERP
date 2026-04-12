using com.etsoo.Utils.Actions;
using CRM.Server.Dto.OrderLine;
using CRM.Server.RQ.OrderLine;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IOrderLineService
    {
        Task<IActionResult> CreateAsync(OrderLineCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task ListAsync(OrderLineListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<OrderLineQueryData[]> QueryAsync(OrderLineQueryRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> StartAsync(OrderLineStartRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(OrderLineUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<OrderLineUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default);
    }
}
