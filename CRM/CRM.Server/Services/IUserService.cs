using CRM.Server.Dto.User;
using CRM.Server.RQ.User;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IUserService
    {
        Task ListAsync(UserListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<UserQueryData[]> QueryAsync(UserQueryRQ rq, CancellationToken cancellationToken = default);
    }
}