
namespace Platform.Server.Services
{
    public interface IStorageService
    {
        Task DownloadFileAsync(string path, int? orgId, CancellationToken cancellationToken = default);
        Task DownloadOrgFileAsync(string path, CancellationToken cancellationToken = default);
        Task DownloadUserFileAsync(string path, string userId, CancellationToken cancellationToken = default);
        ValueTask EditorStylesAsync(CancellationToken cancellationToken = default);
        Task ProfileAttachmentAsync(long id, long timestamp, string key, CancellationToken cancellationToken = default);
    }
}