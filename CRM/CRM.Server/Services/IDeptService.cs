using com.etsoo.Utils.Actions;
using CRM.Server.RQ.Dept;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IDeptService
    {
        Task<IActionResult> CreateAsync(DeptCreateRQ rq, CancellationToken cancellationToken = default);
        Task ListAsync(DeptListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(DeptQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(DeptUpdateRQ rq, CancellationToken cancellationToken = default);
        Task UpdateReadAsync(long id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}