using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.User;
using CRM.Server.Dto.Person;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Threading;

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

        private static (IdentityTypeFlags, bool) GetIdentityType(bool[] permissions)
        {
            var type = IdentityTypeFlags.None;
            var count = 0;

            if (permissions[0])
            {
                type |= IdentityTypeFlags.User;
                count++;
            }

            if (permissions[1])
            {
                type |= IdentityTypeFlags.Customer;
                count++;
            }

            if (permissions[2])
            {
                type |= IdentityTypeFlags.Supplier;
                count++;
            }

            if (permissions[3])
            {
                type |= IdentityTypeFlags.Org;
                count++;
            }

            if (permissions[4])
            {
                type |= IdentityTypeFlags.Dept;
                count++;
            }

            return (type, count == permissions.Length);
        }

        public async Task AddOrUpdatePersonInfoAsync(long personId, PersonInfoKind kind, string? identifier, CancellationToken cancellationToken = default)
        {
            var pinInfo = await _db.PersonInfos
                .Where(i => i.PersonId == personId && i.Kind == kind)
                .OrderByDescending(i => i.IsDefault)
                .FirstOrDefaultAsync(cancellationToken);

            identifier = identifier?.Trim().ToLower();

            if (string.IsNullOrEmpty(identifier))
            {
                if (pinInfo != null)
                {
                    _db.PersonInfos.Remove(pinInfo);
                }
            }
            else
            {
                if (pinInfo != null)
                {
                    pinInfo.Identifier = identifier;

                    // Remove verification status
                    pinInfo.IsVerified = null;
                }
                else
                {
                    pinInfo = new PersonInfo
                    {
                        PersonId = personId,
                        Kind = kind,
                        Identifier = identifier,
                        IsDefault = true
                    };

                    _db.PersonInfos.Add(pinInfo);
                }
            }
        }

        /// <summary>
        /// Add tags
        /// 添加标签
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <param name="tags">Tags</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tag ids</returns>
        public Task<int[]> AddTagsAsync(FeatureTagKind kind, IEnumerable<string> tags, CancellationToken cancellationToken = default)
        {
            var orgId = _userAccessor.UserSafe.OrganizationInt;

            var orgIdSP = new NpgsqlParameter<int>("p_org_id", orgId);
            var kindSP = new NpgsqlParameter<short>("p_kind", (short)kind);
            var tagsSP = new NpgsqlParameter<string[]>("p_tags", [.. tags]);

            return _db.Database
                .SqlQuery<int>($"SELECT * FROM add_tags({orgIdSP}, {kindSP}, {tagsSP})")
                .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Get person's permission identity type
        /// 获取个人的权限身份类型
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<(IdentityTypeFlags, bool)> GetPersonIdentityTypeAsync(CancellationToken cancellationToken = default)
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
        public async Task<(IdentityTypeFlags, bool)> GetProfileIdentityTypeAsync(CancellationToken cancellationToken = default)
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
        /// Get tag kind from identity type
        /// 从身份类型获取标签类型
        /// </summary>
        /// <param name="type">Identity type</param>
        /// <returns>Tag kind</returns>
        public FeatureTagKind GetTagKind(IdentityTypeFlags type)
        {
            if (type.HasFlag(IdentityTypeFlags.User))
                return FeatureTagKind.User;
            else if (type.HasFlag(IdentityTypeFlags.Customer))
                return FeatureTagKind.Customer;
            else if (type.HasFlag(IdentityTypeFlags.Supplier))
                return FeatureTagKind.Supplier;
            else if (type.HasFlag(IdentityTypeFlags.Org))
                return FeatureTagKind.Org;
            else if (type.HasFlag(IdentityTypeFlags.Dept))
                return FeatureTagKind.Dept;
            else
                return FeatureTagKind.Contact;
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
        /// Read tag id by tag and organization id
        /// 通过标签和机构编号读取标签编号
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<int> ReadTagIdAsync(string tag, int orgId, CancellationToken cancellationToken = default)
        {
            return _db.FeatureTags
                .AsNoTracking()
                .Where(t => t.CoreOrganizationId == orgId && t.Tag == tag)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Update person tag
        /// 更新人员标签
        /// </summary>
        /// <param name="tag">Tag data</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async ValueTask UpdatePersonTagAsync(IPersonTag tag, int orgId, CancellationToken cancellationToken = default)
        {
            if (tag.TagId == null && !string.IsNullOrEmpty(tag.Tag))
            {
                tag.TagId = await ReadTagIdAsync(tag.Tag, orgId, cancellationToken);
            }
        }
    }
}
