using com.etsoo.Utils.Actions;
using Platform.Server.Dto.Document;
using Platform.Server.Endpoints.Document.RQ;
using PlatformShared.Dto;

namespace Platform.Server.Services
{
    public interface IDocumentService
    {
        Task<IActionResult> CreateAsync(DocumentCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<DocumentListData[]> ListAsync(DocumentListRQ rq, CancellationToken cancellationToken = default);
        Task<DocumentQueryData[]> QueryAsync(DocumentQueryRQ rq, CancellationToken cancellationToken = default);
        Task<DocumentReadData?> ReadAsync(int id, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(DocumentUpdateRQ rq, CancellationToken cancellationToken = default);
    }
}