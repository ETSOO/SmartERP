using Admin.Server.Dto.Document;
using Admin.Server.RQ.Document;
using com.etsoo.Utils.Actions;

namespace Admin.Server.Services
{
    public interface IDocumentService
    {
        Task<IActionResult> CreateAsync(DocumentCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<DocumentQueryData[]> QueryAsync(DocumentQueryRQ rq, CancellationToken cancellationToken = default);
        Task<DocumentViewData?> ReadAsync(int id, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(DocumentUpdateRQ rq, CancellationToken cancellationToken = default);
    }
}