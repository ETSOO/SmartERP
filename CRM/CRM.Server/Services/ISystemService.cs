using com.etsoo.Utils.Actions;
using CRM.Server.Dto.System;
using CRM.Server.RQ.System;

namespace CRM.Server.Services
{
    public interface ISystemService
    {
        Task<PermissionItem[]> PermissionItemsAsync(CancellationToken cancellationToken = default);
        Task<CustomCultureItem?> ReadCultureAsync(ReadCultureRQ rq, CancellationToken cancellationToken = default);
        Task<SystemSettings?> ReadSettingsAsync(CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateCultureAsync(UpdateCultureRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateSettingsAsync(UpdateSettingsRQ rq, CancellationToken cancellationToken = default);
    }
}