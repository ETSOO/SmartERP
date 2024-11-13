
using Platform.Server.Dto.App;

namespace Platform.Server.Services
{
    public interface IAppService
    {
        Task<IEnumerable<AppData>> GetUserAppsAsync(CancellationToken cancellationToken = default);
    }
}