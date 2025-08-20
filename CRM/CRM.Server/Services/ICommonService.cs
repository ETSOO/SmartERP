
using com.etsoo.CoreFramework.Business;
using CRM.Server.Dto.Person;
using PlatformShared.Database.Models;

namespace CRM.Server.Services
{
    public interface ICommonService
    {
        Task<int[]> AddTagsAsync(FeatureTagKind kind, IEnumerable<string> tags, CancellationToken cancellationToken = default);
        Task<IdentityTypeFlags> GetPersonIdentityTypeAsync(CancellationToken cancellationToken = default);
        Task<IdentityTypeFlags> GetProfileIdentityTypeAsync(CancellationToken cancellationToken = default);
        FeatureTagKind GetTagKind(IdentityTypeFlags type);
        ValueTask<bool> HasIdentityPermissionAsync(IdentityTypeFlags identityType, string name, CancellationToken cancellationToken);
        Task<bool> HasPermissionAsync(short permissionItemId, CancellationToken cancellationToken = default);
        Task<bool[]> HasPermissionsAsync(IEnumerable<short> permissionItemIds, CancellationToken cancellationToken = default);
        IdentityTypeFlags MergeIdentityType(IdentityTypeFlags? current, IdentityTypeFlags range);
        Task<int> ReadTagIdAsync(string tag, int orgId, CancellationToken cancellationToken = default);
        ValueTask UpdatePersonTagAsync(IPersonTag tag, int orgId, CancellationToken cancellationToken = default);
    }
}