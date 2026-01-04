
using com.etsoo.CoreFramework.Business;
using CRM.Server.Dto.Person;
using PlatformShared.Database.Models;

namespace CRM.Server.Services
{
    public interface ICommonService
    {
        Task AddOrUpdatePersonInfoAsync(long personId, PersonInfoKind kind, string? identifier, CancellationToken cancellationToken = default);
        Task<int[]> AddTagsAsync(FeatureTagKind kind, IEnumerable<string> tags, CancellationToken cancellationToken = default);
        Task<(IdentityTypeFlags, bool)> GetPersonIdentityTypeAsync(CancellationToken cancellationToken = default);
        Task<(IdentityTypeFlags, bool)> GetProfileIdentityTypeAsync(CancellationToken cancellationToken = default);
        FeatureTagKind GetTagKind(IdentityTypeFlags type);
        ValueTask<bool> HasIdentityPermissionAsync(IdentityTypeFlags identityType, string name, CancellationToken cancellationToken);
        Task<bool> HasPermissionAsync(short permissionItemId, CancellationToken cancellationToken = default);
        Task<bool[]> HasPermissionsAsync(IEnumerable<short> permissionItemIds, CancellationToken cancellationToken = default);
        Task<int> ReadTagIdAsync(string tag, int orgId, CancellationToken cancellationToken = default);
        ValueTask UpdatePersonTagAsync(IPersonTag tag, int orgId, CancellationToken cancellationToken = default);
    }
}