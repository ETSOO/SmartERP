
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto;
using CRM.Server.Dto.PersonProfile;
using CRM.Server.Dto.System;
using PlatformShared.Database.Models;

namespace CRM.Server.Services
{
    public interface ICommonService
    {
        Task AddOrUpdatePersonInfoAsync(long personId, PersonInfoKind kind, string? identifier, CancellationToken cancellationToken = default);
        Task AddProfileAsync(PersonProfileAction action, CancellationToken cancellationToken = default);
        Task<int[]> AddTagsAsync(FeatureTagKind kind, IEnumerable<string> tags, CancellationToken cancellationToken = default);
        string GetCultureKey(long id, CustomCultureKind kind);
        Task<string?> GetDefaultCulture(int orgId, CancellationToken cancellationToken = default);
        Task<string?> GetDefaultCurrency(int orgId, CancellationToken cancellationToken = default);
        Task<(IdentityTypeFlags, bool)> GetPersonIdentityTypeAsync(CancellationToken cancellationToken = default);
        Task<(IdentityTypeFlags, bool)> GetProfileIdentityTypeAsync(CancellationToken cancellationToken = default);
        FeatureTagKind GetTagKind(IdentityTypeFlags type);
        ValueTask<bool> HasIdentityPermissionAsync(IdentityTypeFlags identityType, string name, CancellationToken cancellationToken);
        Task<bool> HasPermissionAsync(short permissionItemId, CancellationToken cancellationToken = default);
        Task<bool[]> HasPermissionsAsync(IEnumerable<short> permissionItemIds, CancellationToken cancellationToken = default);
        Task<int> ReadTagIdAsync(string tag, int orgId, CancellationToken cancellationToken = default);
        Task<IActionResult> SyncAssetAsync(long personId, int assetId, int assetQty, decimal qty, CancellationToken cancellationToken = default);
        ValueTask UpdateTagAsync(IQueryTag tag, int orgId, CancellationToken cancellationToken = default);
        ValueTask<(ActionResult result, IEnumerable<int>? ids)> ValidatePersonCategoriesAsync(IEnumerable<int>? ids, int orgId, CancellationToken cancellationToken = default);
        ValueTask<(ActionResult result, IEnumerable<int>? ids)> ValidateProductCategoriesAsync(IEnumerable<int>? ids, int orgId, CancellationToken cancellationToken = default);
    }
}