using com.etsoo.Utils.Actions;
using CRM.Server.Dto.User;
using CRM.Server.RQ.User;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IUserService
    {
        Task ListAsync(UserListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<UserQueryData[]> QueryAsync(UserQueryRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(UserUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<UserUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default);
        Task UpdateReadAsync(long id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}