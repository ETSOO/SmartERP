using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.User;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Messages;

namespace PlatformShared.Extentions
{
    /// <summary>
    /// Shared extentions
    /// 共享扩展
    /// </summary>
    public static class SharedExtentions
    {
        /// <summary>
        /// Query users from persons
        /// 从人员中查询用户
        /// </summary>
        /// <param name="persons">Persons</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <returns>Result</returns>
        public static IQueryable<Person> Users(this IQueryable<Person> persons, int orgId)
        {
            return persons.Where(p => p.OrgId == orgId
                && p.CoreUserId != null
                && p.IdentityType.HasValue
                && p.IdentityType.Value.HasFlag(IdentityTypeFlags.User)
            );
        }

        /// <summary>
        /// Query customers from persons
        /// 从人员中查询客户
        /// </summary>
        /// <param name="persons">Persons</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <returns>Result</returns>
        public static IQueryable<Person> Customers(this IQueryable<Person> persons, int orgId)
        {
            return persons.Where(p => p.OrgId == orgId
                && p.IdentityType.HasValue
                && p.IdentityType.Value.HasFlag(IdentityTypeFlags.Customer)
            );
        }

        /// <summary>
        /// Query suppliers from persons
        /// 从人员中查询供应商
        /// </summary>
        /// <param name="persons">Persons</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <returns>Result</returns>
        public static IQueryable<Person> Suppliers(this IQueryable<Person> persons, int orgId)
        {
            return persons.Where(p => p.OrgId == orgId
                && p.IdentityType.HasValue
                && p.IdentityType.Value.HasFlag(IdentityTypeFlags.Supplier)
            );
        }

        /// <summary>
        /// Query contacts from persons
        /// 从人员中查询联系人
        /// </summary>
        /// <param name="persons">Persons</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <param name="ownerId">Owner id</param>
        /// <returns>Result</returns>
        public static IQueryable<Person> Contacts(this IQueryable<Person> persons, int orgId, long ownerId)
        {
            return persons.Where(p => p.OrgId == orgId
                && p.OwnerId == ownerId
            );
        }

        /// <summary>
        /// Create common message data
        /// 创建通用消息数据
        /// </summary>
        /// <param name="user">Current user</param>
        /// <param name="appId">Application id</param>
        /// <param name="targetId">Target id</param>
        /// <param name="targetName">Target name</param>
        /// <returns>Result</returns>
        public static CommonMessageData CreateMessageData(this CurrentUser user, int appId, long targetId, string? targetName = null)
        {
            return new CommonMessageData
            {
                AppId = appId,
                Culture = user.Language.Name,
                DeviceId = user.DeviceIdInt,
                IP = user.ClientIp.ToString(),
                UserId = user.IdInt,
                UserName = user.Name,
                OrganizationId = user.OrganizationInt,
                TimeZone = (user.TimeZone ?? TimeZoneInfo.Local).Id,
                TargetId = targetId,
                TargetName = targetName
            };
        }

        /// <summary>
        /// Query users by role
        /// 通过角色查询用户
        /// </summary>
        /// <param name="db">EF db</param>
        /// <param name="orgId">Organization belonged id</param>
        /// <param name="role">User role</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Id array</returns>
        public static Task<List<int>> QueryUsersAsync(this MyDbContext db, int orgId, UserRole role, CancellationToken cancellationToken = default)
        {
            return db.Persons.AsNoTracking()
                .Users(orgId)
                .Where(ou => ou.UserRole >= role && ou.Status <= EntityStatus.Approved)
                .Select(ou => ou.CoreUserId!.Value)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Query user identifiers by type and ids
        /// 通过类型和编号查询用户唯一信息
        /// </summary>
        /// <param name="db">EF db</param>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="ids">Ids</param>
        /// <returns>Result</returns>
        public static async Task<string[][]> QueryUserIdentifiersAsync(this MyDbContext db, CoreUserIdentifierType type, CancellationToken cancellationToken = default, params IEnumerable<int>[] ids)
        {
            // Flatten all ids into a single list to query the database in one go
            var allIds = ids.SelectMany(i => i).Distinct().ToList();

            // Query CoreUserIdentifiers for all the ids in one go
            var userIdentifiers = await db.CoreUserIdentifiers
                .AsNoTracking()
                .Where(i => allIds.Contains(i.CoreUserId) && i.Type == type)
                .Select(i => new { i.CoreUserId, i.Value })
                .ToListAsync(cancellationToken);

            // Initialize the result array
            var result = new string[ids.Length][];

            for (var i = 0; i < ids.Length; i++)
            {
                var currentIds = ids[i];

                // Filter
                var emails = userIdentifiers
                    .Where(i => currentIds.Contains(i.CoreUserId))
                    .Select(i => i.Value)
                    .ToArray();

                // Assign the filtered emails to the result array
                result[i] = emails;
            }

            // Return
            return result;
        }
    }
}
