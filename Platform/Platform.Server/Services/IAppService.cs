
using Platform.Server.Dto.App;

namespace Platform.Server.Services
{
    public interface IAppService
    {
        Task<string> GetUserLatestAppAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<AppData>> GetUserAppsAsync(CancellationToken cancellationToken = default);
    }
}