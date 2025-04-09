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
        /// Check person ids
        /// 检查人员编号
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="ids">Person ids</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async Task<bool> CheckPersonsAsync(this MyDbContext db, IEnumerable<long> ids, int orgId, CancellationToken cancellationToken = default)
        {
            ids = ids.Distinct();

            var items = ids.Count();
            if (items == 0)
                return true;

            var count = await db.Persons.AsNoTracking()
                .Where(p => p.OrgId == orgId && ids.Contains(p.Id))
                .CountAsync(cancellationToken);

            return count == items;
        }

        /// <summary>
        /// Check order / purchase ids
        /// 检查订单 / 采购编号
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="ids">Orders ids</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async Task<bool> CheckOrdersAsync(this MyDbContext db, IEnumerable<long> ids, int orgId, CancellationToken cancellationToken = default)
        {
            var count = await db.OrderHeaders.AsNoTracking()
                .Where(o => o.CoreOrganizationId == orgId && ids.Contains(o.Id))
                .CountAsync(cancellationToken);

            return count == ids.Count();
        }

        /// <summary>
        /// Query person profiles by user
        /// 通过用户查询人员档案
        /// </summary>
        /// <param name="profiles">Person profiles</param>
        /// <param name="user">Current user</param>
        /// <returns>Result</returns>
        public static IQueryable<PersonProfile> UserProfiles(this IQueryable<PersonProfile> profiles, CurrentUser user)
        {
            var oid = user.Oid;
            return profiles.Where(p => p.Person.OrgId == user.OrganizationInt
                && (p.UserId == oid || p.AssigneeId == oid || p.UserRole == null || p.UserRole <= user.Role));
        }

        /// <summary>
        /// Query person profiles by user
        /// 通过用户查询人员档案
        /// </summary>
        /// <param name="profiles">Person profiles</param>
        /// <param name="user">Current user</param>
        /// <param name="id">Id</param>
        /// <returns>Result</returns>
        public static IQueryable<PersonProfile> UserProfiles(this IQueryable<PersonProfile> profiles, CurrentUser user, long id)
        {
            return profiles.Where(p => p.Id == id && p.Person.OrgId == user.OrganizationInt
                && (p.UserRole == null || p.UserRole <= user.Role));
        }

        /// <summary>
        /// Query person profile editable attachments by user
        /// 通过用户查询人员档案可编辑的附件
        /// </summary>
        /// <param name="attachments">Attachments</param>
        /// <param name="user">Current user</param>
        /// <param name="id">Attachment id</param>
        /// <returns>Result</returns>
        public static IQueryable<PersonProfileAttachment> CheckAttachmentEditable(this IQueryable<PersonProfileAttachment> attachments, CurrentUser user, long id)
        {
            var isAdmin = user.Role >= UserRole.Admin;
            var oid = user.Oid;
            var orgId = user.OrganizationInt;

            return attachments.Where(a => a.Id == id
                && a.Profile.Person.OrgId == orgId
                && (isAdmin || a.UserId == oid || a.Profile.UserId == oid));
        }

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
