using Admin.Server.RQ.Operation;
using com.etsoo.Utils.Actions;

namespace Admin.Server.Services
{
    public interface IAdminService
    {
        Task<IActionResult> AppRenewAsync(AppRenewRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> ClearUserFrozenAsync(int userId, CancellationToken cancellationToken = default);
    }
}