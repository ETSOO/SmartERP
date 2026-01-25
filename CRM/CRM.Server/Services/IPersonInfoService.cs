using com.etsoo.Utils.Actions;
using CRM.Server.RQ.PersonInfo;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IPersonInfoService
    {
        Task<IActionResult> CreateAsync(PersonInfoCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task QueryAsync(PersonInfoQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<string?> ReadAsync(int id, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(PersonInfoUpdateRQ rq, CancellationToken cancellationToken = default);
    }
}