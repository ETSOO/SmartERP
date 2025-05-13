using com.etsoo.Utils.Actions;
using CRM.Server.Dto.System;
using CRM.Server.RQ.System;
using PlatformShared.Dto;

namespace CRM.Server.Services
{
    public interface ISystemService
    {
        Task<PermissionItem[]> PermissionItemsAsync(CancellationToken cancellationToken = default);
        Task<SystemSettings?> ReadSettingsAsync(CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateSettingsAsync(UpdateSettingsRQ rq, CancellationToken cancellationToken = default);
    }
}