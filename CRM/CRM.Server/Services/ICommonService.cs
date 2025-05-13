
using com.etsoo.CoreFramework.Business;

namespace CRM.Server.Services
{
    public interface ICommonService
    {
        Task<IdentityTypeFlags> GetPersonIdentityTypeAsync(CancellationToken cancellationToken = default);
        Task<IdentityTypeFlags> GetProfileIdentityTypeAsync(CancellationToken cancellationToken = default);
        ValueTask<bool> HasIdentityPermissionAsync(IdentityTypeFlags identityType, string name, CancellationToken cancellationToken);
        Task<bool> HasPermissionAsync(short permissionItemId, CancellationToken cancellationToken = default);
        Task<bool[]> HasPermissionsAsync(IEnumerable<short> permissionItemIds, CancellationToken cancellationToken = default);
        IdentityTypeFlags MergeIdentityType(IdentityTypeFlags? current, IdentityTypeFlags range);
    }
}