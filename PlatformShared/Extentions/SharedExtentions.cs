using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Utils.Actions;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
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

        private static short GetBaseId(this short permissionId)
        {
            return (short)(permissionId / 1000 * 1000);
        }

        /// <summary>
        /// Check if the user has permission
        /// 检查用户是否有权限
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="personId">User person id</param>
        /// <param name="permissionItemId">Permission item id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async Task<bool> HasPermissionAsync(this MyDbContext db, long personId, short permissionItemId, CancellationToken cancellationToken = default)
        {
            var ps = await HasPermissionsAsync(db, personId, [permissionItemId], cancellationToken);
            return ps[0];
        }

        /// <summary>
        /// Check if the user has permission
        /// 检查用户是否有权限
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="personId">User person id</param>
        /// <param name="permissionItemIds">Permission item ids</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async Task<bool[]> HasPermissionsAsync(this MyDbContext db, long personId, IEnumerable<short> permissionItemIds, CancellationToken cancellationToken = default)
        {
            // All ids to check
            var allIds = permissionItemIds
                .SelectMany(i => new[] { i.GetBaseId(), i })
                .Distinct()
                .ToList();

            // Query all permissions in one go
            var permissions = await db.PersonPermissionItems.AsNoTracking()
                .Where(p => p.PersonId == personId && allIds.Contains(p.PermissionItemId))
                .Select(p => p.PermissionItemId)
                .ToArrayAsync(cancellationToken);

            // Check if all permissionItemIds are in permissions
            return [.. permissionItemIds.Select(p => permissions.Contains(p.GetBaseId()) || permissions.Contains(p))];
        }

        /// <summary>
        /// Query assets
        /// 查询资产
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id</param>
        /// <returns>Result</returns>
        public static IQueryable<PersonAsset> Assets(this MyDbContext db, int orgId)
        {
            return db.PersonAssets.Where(p => p.OrgId == orgId);
        }

        /// <summary>
        /// Query persons
        /// 查询人员
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <returns>Result</returns>
        public static IQueryable<Person> Persons(this MyDbContext db, int orgId)
        {
            return db.Persons.Where(p => p.OrgId == orgId);
        }

        /// <summary>
        /// Query person addresses
        /// 查询人员地址
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="personId">Person id</param>
        /// <returns>Result</returns>
        public static IQueryable<PersonAddress> PersonAddresses(this MyDbContext db, long personId)
        {
            return db.PersonAddresses
                .Where(r => r.PersonId == personId);
        }

        /// <summary>
        /// Query person relations
        /// 查询人员关系
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <param name="personId">Person id</param>
        /// <returns>Result</returns>
        public static IQueryable<PersonRelation> PersonRelations(this MyDbContext db, int orgId, long personId)
        {
            return db.PersonRelations
                .Include(r => r.Contact)
                .Where(r => r.PersonId == personId && r.Person.OrgId == orgId);
        }

        /// <summary>
        /// Query person profiles by user
        /// 通过用户查询人员档案
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="user">Current user</param>
        /// <returns>Result</returns>
        public static IQueryable<PersonProfile> UserProfiles(this MyDbContext db, CurrentUser user)
        {
            var oid = user.Oid;
            return db.PersonProfiles.Where(p => p.Person.OrgId == user.OrganizationInt
                && (p.UserId == oid || p.AssigneeId == oid || p.UserRole == null || p.UserRole <= user.Role));
        }

        /// <summary>
        /// Query person profiles by user
        /// 通过用户查询人员档案
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="user">Current user</param>
        /// <param name="id">Person id</param>
        /// <returns>Result</returns>
        public static IQueryable<PersonProfile> UserProfiles(this MyDbContext db, CurrentUser user, long id)
        {
            return db.PersonProfiles.Where(p => p.Id == id && p.Person.OrgId == user.OrganizationInt
                && (p.UserRole == null || p.UserRole <= user.Role));
        }

        /// <summary>
        /// Query person profiles by users
        /// 通过用户查询人员档案
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="user">Current user</param>
        /// <param name="ids">Perons ids</param>
        /// <returns>Result</returns>
        public static IQueryable<PersonProfile> UserProfiles(this MyDbContext db, CurrentUser user, List<long> ids)
        {
            return db.PersonProfiles.Where(p => ids.Contains(p.Id) && p.Person.OrgId == user.OrganizationInt
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
        /// Query person profile editable links by user
        /// 通过用户查询人员档案可编辑的链接
        /// </summary>
        /// <param name="attachments">Attachments</param>
        /// <param name="user">Current user</param>
        /// <param name="id">Attachment id</param>
        /// <returns>Result</returns>
        public static IQueryable<PersonProfileLink> CheckLinkEditable(this IQueryable<PersonProfileLink> links, CurrentUser user, long id)
        {
            var isAdmin = user.Role >= UserRole.Admin;
            var oid = user.Oid;
            var orgId = user.OrganizationInt;

            return links.Where(l => l.Id == id
                && l.Profile.Person.OrgId == orgId
                && (isAdmin || l.UserId == oid || l.Profile.UserId == oid));
        }

        /// <summary>
        /// Get related target
        /// 获取关联对象
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Result</returns>
        public static string GetRelatedTarget(this IdentityTypeData data)
        {
            var type = data.IdentityType;
            var typeMappings = new (IdentityTypeFlags Flag, string Name)[]
            {
                (IdentityTypeFlags.User, Resources.User),
                (IdentityTypeFlags.Customer, Resources.Customer),
                (IdentityTypeFlags.Supplier, Resources.Supplier),
                (IdentityTypeFlags.Org, Resources.Org),
                (IdentityTypeFlags.None, Resources.Contact)
            };

            var types = typeMappings
                .Where(m => type.HasFlag(m.Flag))
                .Select(m => m.Name)
                .ToList();

            var label = $"[{string.Join(", ", types)}] {data.Name}";

            if (type == IdentityTypeFlags.None && data.Owner != null)
            {
                label += $" / {data.Owner.Name}";
            }

            return label;
        }

        /// <summary>
        /// Query users
        /// 查询用户
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <returns>Result</returns>
        public static IQueryable<Person> Users(this MyDbContext db, int orgId)
        {
            return db.Persons.Where(p => p.OrgId == orgId
                && p.CoreUserId != null
                && p.IdentityType.HasFlag(IdentityTypeFlags.User)
            );
        }

        /// <summary>
        /// Query customers
        /// 查询客户
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <returns>Result</returns>
        public static IQueryable<Person> Customers(this MyDbContext db, int orgId)
        {
            return db.Persons.Where(p => p.OrgId == orgId
                && p.IdentityType.HasFlag(IdentityTypeFlags.Customer)
            );
        }

        /// <summary>
        /// Query department
        /// 查询部门
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <returns>Result</returns>
        public static IQueryable<Person> Depts(this MyDbContext db, int orgId)
        {
            return db.Persons.Where(p => p.OrgId == orgId
                && p.IdentityType.HasFlag(IdentityTypeFlags.Dept)
            );
        }

        /// <summary>
        /// Query permission groups
        /// 查询权限组
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <returns>Result</returns>
        public static IQueryable<PermissionGroup> Groups(this MyDbContext db, int orgId)
        {
            return db.PermissionGroups.Where(g => g.CoreOrganizationId == null
                || g.CoreOrganizationId == orgId
            );
        }

        /// <summary>
        /// Query orders
        /// 查询订单
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Current organization</param>
        /// <returns>Result</returns>
        public static IQueryable<OrderHeader> Orders(this MyDbContext db, int orgId)
        {
            return db.OrderHeaders.Where(p => p.CoreOrganizationId == orgId && p.IsOrder);
        }

        /// <summary>
        /// Query orders & pos
        /// 查询订单和采购
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Current organization</param>
        /// <returns>Result</returns>
        public static IQueryable<OrderHeader> OrderAndPOs(this MyDbContext db, int orgId)
        {
            return db.OrderHeaders.Where(p => p.CoreOrganizationId == orgId);
        }

        /// <summary>
        /// Query person categories
        /// 查询人员分类
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <param name="identityType">Identity type</param>
        /// <returns>Result</returns>
        public static IQueryable<PersonCategory> PersonCategories(this MyDbContext db, int orgId, IdentityTypeFlags? identityType = null)
        {
            if (identityType.HasValue)
            {
                return db.PersonCategories.Where(p => p.CoreOrganizationId == orgId
                    && (p.IdentityType & identityType) > 0
                );
            }
            else
            {
                return db.PersonCategories.Where(p => p.CoreOrganizationId == orgId);
            }
        }

        /// <summary>
        /// Query purchase orders
        /// 查询采购
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id</param>
        /// <returns>Result</returns>
        public static IQueryable<OrderHeader> POs(this MyDbContext db, int orgId)
        {
            return db.OrderHeaders.Where(p => p.CoreOrganizationId == orgId && !p.IsOrder);
        }

        /// <summary>
        /// Query products
        /// 查询产品
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id</param>
        /// <returns>Result</returns>
        public static IQueryable<Product> Products(this MyDbContext db, int orgId)
        {
            return db.Products.Where(p => p.CoreOrganizationId == orgId);
        }

        /// <summary>
        /// Query product categories
        /// 查询产品分类
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <returns>Result</returns>
        public static IQueryable<ProductCategory> ProductCategories(this MyDbContext db, int orgId)
        {
            return db.ProductCategories.Where(p => p.CoreOrganizationId == orgId);
        }

        /// <summary>
        /// Query order deliveries
        /// 查询订单配送方式
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id</param>
        /// <returns>Result</returns>
        public static IQueryable<OrderDelivery> OrderDeliveries(this MyDbContext db, int orgId)
        {
            return db.OrderDeliveries.Where(p => p.CoreOrganizationId == orgId);
        }

        /// <summary>
        /// Query order payments
        /// 查询订单支付方式
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id</param>
        /// <returns>Result</returns>
        public static IQueryable<OrderPayment> OrderPayments(this MyDbContext db, int orgId)
        {
            return db.OrderPayments.Where(p => p.CoreOrganizationId == orgId);
        }

        /// <summary>
        /// Query promotions
        /// 查询促销
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id</param>
        /// <returns>Result</returns>
        public static IQueryable<Promotion> Promotions(this MyDbContext db, int orgId)
        {
            return db.Promotions.Where(p => p.CoreOrganizationId == orgId);
        }

        /// <summary>
        /// Query suppliers from persons
        /// 从人员中查询供应商
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="orgId">Organization id belonged</param>
        /// <returns>Result</returns>
        public static IQueryable<Person> Suppliers(this MyDbContext db, int orgId)
        {
            return db.Persons.Where(p => p.OrgId == orgId
                && p.IdentityType.HasFlag(IdentityTypeFlags.Supplier)
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
        /// Check duplicate for multiple kind/identifier pairs
        /// 支持多个 (Kind, Identifier) 组合检查
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="orgId">Organization</param>
        /// <param name="excludedId">Excluded person id</param>
        /// <param name="items">Kind/Identifier pairs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public static async ValueTask<IActionResult> PersonInfoDuplicateAsync(this MyDbContext db, int orgId, long? excludedId, IEnumerable<(PersonInfoKind kind, string identifier)> items, CancellationToken cancellationToken = default)
        {
            if (!items.Any())
                return ActionResult.Success;

            var kinds = items.Select(i => i.kind).Distinct();
            var identifiers = items.Select(i => i.identifier.ToLower()).Distinct();

            var mayItems = await db.PersonInfos.AsNoTracking()
                .Where(pi => pi.Person.OrgId == orgId && (excludedId == null || pi.PersonId != excludedId)
                    && kinds.Contains(pi.Kind) && identifiers.Contains(pi.Identifier))
                .Select(pi => new { pi.Kind, pi.Identifier })
                .ToArrayAsync(cancellationToken);

            if (mayItems.Any(m => items.Any(i => i.kind == m.Kind && i.identifier.Equals(m.Identifier, StringComparison.OrdinalIgnoreCase))))
                return ApplicationErrors.ItemExists.AsResult(string.Join(',', kinds));
            else
                return ActionResult.Success;
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
            return db.Users(orgId).AsNoTracking()
                .Where(ou => ou.UserRole >= role && ou.Status <= EntityStatus.Approved)
                .Select(ou => ou.CoreUserId!.Value)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Query person identifiers by type and ids
        /// 通过类型和编号查询成员唯一信息
        /// </summary>
        /// <param name="db">EF db</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="ids">Ids</param>
        /// <returns>Result</returns>
        public static async Task<string[][]> QueryPersonIdentifiersAsync(this MyDbContext db, int orgId, CoreUserIdentifierType type, CancellationToken cancellationToken = default, params IEnumerable<long>[] ids)
        {
            // Map CoreUserIdentifierType to PersonInfoKind
            PersonInfoKind? kind = type switch
            {
                CoreUserIdentifierType.Email => PersonInfoKind.Email,
                CoreUserIdentifierType.Mobile => PersonInfoKind.Mobile,
                CoreUserIdentifierType.Wechat => PersonInfoKind.WeChat,
                _ => null
            };

            // Flatten all ids into a single list to query the database in one go
            var allIds = ids.SelectMany(i => i).Distinct().ToList();

            // Query for all the ids in one go
            var users = db.Persons.AsNoTracking()
                .Where(p => p.OrgId == orgId && allIds.Contains(p.Id) && p.CoreUser != null)
                .SelectMany(p => p.CoreUser!.CoreUserIdentifiers
                            .Where(i => i.Type == type)
                            .Select(i => new LongIdItem
                            {
                                Id = p.Id,
                                Title = i.Value
                            })
                );

            var contacts = db.Persons.AsNoTracking()
                .Where(p => p.OrgId == orgId && allIds.Contains(p.Id) && p.CoreUser == null)
                .SelectMany(p => p.Infos
                            .Where(i => i.Kind == kind && i.IsDefault)
                            .Select(i => new LongIdItem
                            {
                                Id = p.Id,
                                Title = i.Identifier
                            })
                );

            // Combine the two queries
            var results = await users.Union(contacts)
                .ToListAsync(cancellationToken);

            // Initialize the result array
            var result = new string[ids.Length][];

            for (var i = 0; i < ids.Length; i++)
            {
                var currentIds = ids[i];

                // Filter
                var emails = results
                    .Where(i => currentIds.Contains(i.Id))
                    .Select(i => i.Title)
                    .ToArray();

                // Assign the filtered emails to the result array
                result[i] = emails;
            }

            // Return
            return result;
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
