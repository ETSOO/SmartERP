
using Platform.Server.Dto.App;

namespace Platform.Server.Services
{
    public interface IAppService
    {
        Task<IEnumerable<AppData>> GetAppsAsync(CancellationToken cancellationToken = default);
    }
}