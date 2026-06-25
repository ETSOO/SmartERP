using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Customer;
using CRM.Server.RQ;
using CRM.Server.RQ.Customer;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface ICustomerService
    {
        Task<IActionResult> CreateAsync(CustomerCreateRQ rq, CancellationToken cancellationToken = default);
        Task<AppActionData?> DocumentActionAsync(DocumentActionRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(CustomerListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<CustomerQueryData[]> QueryAsync(CustomerQueryRQ rq, CancellationToken cancellationToken = default);
        Task<CustomerReadForSaleData?> ReadForSaleAsync(CustomerReadForSaleRQ rq, CancellationToken cancellationToken = default);
        Task<AppActionData?> ReportActionAsync(CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(CustomerUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<CustomerUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default);
    }
}