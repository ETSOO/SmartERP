using com.etsoo.Utils.Actions;
using CRM.Server.Dto.OrderPayment;
using CRM.Server.RQ.OrderPayment;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IOrderPaymentService
    {
        Task<IActionResult> CreateAsync(OrderPaymentCreateRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(OrderPaymentListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<OrderPaymentQueryData[]> QueryAsync(OrderPaymentQueryRQ rq, CancellationToken cancellationToken = default);
        Task<int> SortAsync(Dictionary<int, short> rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(OrderPaymentUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<OrderPaymentUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default);
    }
}
