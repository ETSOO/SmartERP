using CRM.Server.RQ.User;
using System.Buffers;

namespace CRM.Server.Services
{
    public interface IUserService
    {
        Task ListAsync(UserListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryAsync(UserQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
    }
}