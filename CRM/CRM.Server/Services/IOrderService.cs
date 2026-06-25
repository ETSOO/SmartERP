using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Order;
using CRM.Server.RQ;
using CRM.Server.RQ.Order;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IOrderService
    {
        Task<(bool IsEdit, bool IsManage)> CheckEditPermissionsAsync(CancellationToken cancellationToken = default);
        Task<IActionResult> CreateAsync(OrderCreateRQ rq, CancellationToken cancellationToken = default);
        Task<AppActionData?> DocumentActionAsync(DocumentActionRQ rq, CancellationToken cancellationToken = default);
        ValueTask<OrderDuplicateTestData[]?> DuplicateTestAsync(OrderDuplicateTestRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(OrderListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<OrderListAllData[]> ListAllAsync(OrderListAllRQ rq, CancellationToken cancellationToken = default);
        Task<OrderQueryData[]> QueryAsync(OrderQueryRQ rq, CancellationToken cancellationToken = default);
        Task<OrderViewData?> ReadAsync(long id, CancellationToken cancellationToken = default);
        Task<IActionResult> RecalculateAsync(long id, bool checkPermission, CancellationToken cancellationToken = default);
        Task<AppActionData?> ReportActionAsync(CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(OrderUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<OrderUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default);
    }
}