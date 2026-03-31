using com.etsoo.Utils.Actions;
using CRM.Server.Dto.OrderDelivery;
using CRM.Server.RQ.OrderDelivery;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IOrderDeliveryService
    {
        Task<IActionResult> CreateAsync(OrderDeliveryCreateRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(OrderDeliveryListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<OrderDeliveryQueryData[]> QueryAsync(OrderDeliveryQueryRQ rq, CancellationToken cancellationToken = default);
        Task<int> SortAsync(Dictionary<int, short> rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(OrderDeliveryUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<OrderDeliveryUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default);
    }
}
