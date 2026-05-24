using PlatformShared.Dto;
using PlatformShared.RQ;

namespace Platform.Server.Services
{
    public interface IDocumentService
    {
        Task<SystemDocumentListData[]> ListAsync(SystemDocumentListRQ rq, CancellationToken cancellationToken = default);
        Task<SystemDocumentViewData?> ReadAsync(int id, CancellationToken cancellationToken = default);
    }
}