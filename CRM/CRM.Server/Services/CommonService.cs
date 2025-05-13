using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.User;
using PlatformShared.Database;
using PlatformShared.Extentions;

namespace CRM.Server.Services
{
    /// <summary>
    /// Common service
    /// 通用服务
    /// </summary>
    public class CommonService : ICommonService
    {
        readonly MyDbContext _db;
        readonly CurrentUserAccessor _userAccessor;

        public CommonService(
            MyDbContext db,
            CurrentUserAccessor userAccessor
        )
        {
            _db = db;
            _userAccessor = userAccessor;
        }

        private static IdentityTypeFlags GetIdentityType(bool[] permissions)
        {
            var type = IdentityTypeFlags.None;

            if (permissions[0])
                type |= IdentityTypeFlags.User;

            if (permissions[1])
                type |= IdentityTypeFlags.Customer;

            if (permissions[2])
                type |= IdentityTypeFlags.Supplier;

            if (permissions[3])
                type |= IdentityTypeFlags.Org;

            if (permissions[4])
                type |= IdentityTypeFlags.Dept;

            if (type != IdentityTypeFlags.None)
            {
                type |= IdentityTypeFlags.Contact;
            }

            return type;
        }

        /// <summary>
        /// Get person's permission identity type
        /// 获取个人的权限身份类型
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IdentityTypeFlags> GetPersonIdentityTypeAsync(CancellationToken cancellationToken = default)
        {
            short[] ids = [
                (short)Permissions.User.Query,
                (short)Permissions.Customer.Query,
                (short)Permissions.Supplier.Query,
                (short)Permissions.Org.Query,
                (short)Permissions.Dept.Query
            ];

            var permissions = await HasPermissionsAsync(ids, cancellationToken);

            return GetIdentityType(permissions);
        }

        /// <summary>
        /// Get profile's permission identity type
        /// 获取个人资料的权限身份类型
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IdentityTypeFlags> GetProfileIdentityTypeAsync(CancellationToken cancellationToken = default)
        {
            short[] ids = [
                (short)Permissions.User.QueryProfile,
                (short)Permissions.Customer.QueryProfile,
                (short)Permissions.Supplier.QueryProfile,
                (short)Permissions.Org.QueryProfile,
                (short)Permissions.Dept.QueryProfile
            ];

            var permissions = await HasPermissionsAsync(ids, cancellationToken);

            return GetIdentityType(permissions);
        }

        /// <summary>
        /// Check if the user has identity permission of the specified item
        /// 检查用户是否有指定身份的权限
        /// </summary>
        /// <param name="identityType">Identity type</param>
        /// <param name="name">Permission item</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<bool> HasIdentityPermissionAsync(IdentityTypeFlags identityType, string name, CancellationToken cancellationToken)
        {
            if (identityType == IdentityTypeFlags.None) return false;

            List<short> ids = [];

            if (identityType.HasFlag(IdentityTypeFlags.User))
            {
                if (Enum.TryParse<Permissions.User>(name, out var user))
                {
                    ids.Add((short)user);
                }
            }

            if (identityType.HasFlag(IdentityTypeFlags.Customer))
            {
                if (Enum.TryParse<Permissions.Customer>(name, out var customer))
                {
                    ids.Add((short)customer);
                }
            }

            if (identityType.HasFlag(IdentityTypeFlags.Supplier))
            {
                if (Enum.TryParse<Permissions.Supplier>(name, out var supplier))
                {
                    ids.Add((short)supplier);
                }
            }

            if (identityType.HasFlag(IdentityTypeFlags.Org))
            {
                if (Enum.TryParse<Permissions.Org>(name, out var org))
                {
                    ids.Add((short)org);
                }
            }

            if (identityType.HasFlag(IdentityTypeFlags.Dept))
            {
                if (Enum.TryParse<Permissions.Dept>(name, out var dept))
                {
                    ids.Add((short)dept);
                }
            }

            if (ids.Count == 0)
            {
                return false;
            }

            var permissions = await HasPermissionsAsync(ids, cancellationToken);

            return permissions.Any(p => p);
        }

        /// <summary>
        /// Check if the user has permission
        /// 检查用户是否有权限
        /// </summary>
        /// <param name="permissionItemId">Permission item id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<bool> HasPermissionAsync(short permissionItemId, CancellationToken cancellationToken = default)
        {
            return _db.HasPermissionAsync(_userAccessor.UserSafe.Oid, permissionItemId, cancellationToken);
        }

        /// <summary>
        /// Check if the user has permissions
        /// 检查用户是否有权限
        /// </summary>
        /// <param name="permissionItemIds">Permission item ids</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<bool[]> HasPermissionsAsync(IEnumerable<short> permissionItemIds, CancellationToken cancellationToken = default)
        {
            return _db.HasPermissionsAsync(_userAccessor.UserSafe.Oid, permissionItemIds, cancellationToken);
        }

        /// <summary>
        /// Merge identity type
        /// 合并身份类型
        /// </summary>
        /// <param name="current">Current type</param>
        /// <param name="range">Max range type</param>
        /// <returns>Result</returns>
        public IdentityTypeFlags MergeIdentityType(IdentityTypeFlags? current, IdentityTypeFlags range)
        {
            if (current == null)
            {
                return range;
            }
            else
            {
                return current.Value & range;
            }
        }
    }
}
