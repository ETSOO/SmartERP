using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Serialization;
using CRM.Server.Application;
using CRM.Server.Dto.User;
using CRM.Server.RQ.User;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Org;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// User service
    /// 用户服务
    /// </summary>
    public class UserService : MyUserService, IUserService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public UserService(
            MyDbContext db,
            IMyApp app,
            MyAppConfiguration config,
            CurrentUserAccessor userAccessor,
            ILogger<UserService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, config, userAccessor.UserSafe, "user", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
        }

        private IQueryable<Person> CreateQuery(UserListRQ rq, Func<IQueryable<Person>, IQueryable<Person>>? filters = null)
        {
            var query = _db.Users(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (ou) => ou.Id, (ou) => ou.Status, (q) =>
                {
                    if (rq.GroupId.HasValue)
                    {
                        q = q.Where(ou => ou.PermissionGroups != null && ou.PermissionGroups.Contains(rq.GroupId.Value));
                    }

                    if (rq.DeptId.HasValue)
                    {
                        q = q.Where(ou => ou.ContactOwners != null && ou.ContactOwners.Any(o => o.PersonId == rq.DeptId && (o.Person.IdentityType & IdentityTypeFlags.Dept) > 0));
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, ou => ou.Name, ou => ou.PreferredName);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.Like(ou.Name, $"%{keyword}%")
                            || (ou.QueryKeyword != null && EF.Functions.Like(ou.QueryKeyword, $"%{keyword}%"))
                            || (ou.PreferredName != null && EF.Functions.Like(ou.PreferredName, $"%{keyword}%"))
                            );
                        }
                    }

                    if (filters != null)
                    {
                        q = filters(q);
                    }

                    return q;
                });

            return query;
        }

        /// <summary>
        /// List person JSON data
        /// 人员列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(UserListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.User.List, cancellationToken))
            {
                return;
            }

            await CreateQuery(rq)
                .Select(ou => new UserListData
                {
                    Id = ou.Id,
                    Name =ou.Name
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query person JSON data
        /// 查询人员JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<UserQueryData[]> QueryAsync(UserQueryRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.User.Query, cancellationToken))
            {
                return [];
            }

            return await CreateQuery(rq)
                .Select(u => new UserQueryData
                {
                    Id = u.Id,
                    Name = u.Name,
                    UserRole = u.UserRole,
                    Depts = u.ContactOwners
                        .Where(o => (o.Person.IdentityType & IdentityTypeFlags.Dept) > 0)
                        .Select(o => o.Person.Name),
                    Status = u.Status,
                    Editable = User.Role >= UserRole.Admin || (u.Id != User.Oid && (u.UserRole == null || u.UserRole <= User.Role)),
                    Creation = u.Creation
                }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Update user
        /// 更新用户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(UserUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.User.Edit, cancellationToken)
                || (rq.Id == User.Oid && User.Role < UserRole.Admin))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var user = await _db.Users(orgId)
                .Include(u => u.ContactOwners).ThenInclude(o => o.Person)
                .Where(u => u.Id == rq.Id && (u.UserRole == null || u.UserRole <= User.Role))
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.ReportTo.HasValue && !await _db.Users(orgId).AnyAsync(u => u.Id == rq.ReportTo.Value, cancellationToken))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.ReportTo));
            }

            if (rq.Depts?.Any() is true && !await _db.Depts(orgId).AnyAsync(d => rq.Depts.Contains(d.Id), cancellationToken))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.Depts));
            }

            if (rq.Groups?.Any() is true && !await _db.Groups(orgId).AnyAsync(g => rq.Groups.Contains(g.Id), cancellationToken))
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.Depts));
            }

            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                user.Name = rq.Name;

                var keyword = ChineseUtils.GetPinyin(rq.Name, true).ToInitials();
                user.QueryKeyword = keyword;
            }

            if (rq.IsModified(nameof(rq.AssignedId)))
            {
                user.AssignedId = rq.AssignedId?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.Expiry)))
            {
                user.Expiry = rq.Expiry?.ToUniversalTime();
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                user.Status = rq.Status.Value;
            }

            if (rq.IsModified(nameof(rq.Data)))
            {
                user.Data = rq.Data;
            }

            if (rq.IsModified(nameof(rq.ReportTo)))
            {
                user.ReportTo = rq.ReportTo;
            }

            if (rq.IsModified(nameof(rq.Categories)))
            {
                // Categories
                var categoryIds = rq.Categories;
                var (result, ids) = await _commonService.ValidatePersonCategoriesAsync(categoryIds, orgId, cancellationToken);
                if (!result.Ok)
                {
                    return result;
                }

                user.CategoryIds = categoryIds?.ToList();
                user.CategoryIdsAll = ids?.ToList();
            }

            if (rq.IsModified(nameof(rq.Depts)))
            {
                if (rq.Depts?.Any() is true)
                {
                    var itemsToAdd = rq.Depts.Where(d => !user.ContactOwners.Any(o => o.PersonId == d))
                        .Select(d => new PersonRelation
                        {
                            PersonId = d,
                            ContactId = rq.Id
                        }).ToList();

                    foreach (var item in itemsToAdd)
                    {
                        user.ContactOwners.Add(item);
                    }
                }

                var itemsToRemove = user.ContactOwners.Where(d => d.Person != null && (d.Person.IdentityType & IdentityTypeFlags.Dept) > 0 && rq.Depts?.Contains(d.PersonId) is not true).ToList();
                foreach (var item in itemsToRemove)
                {
                    user.ContactOwners.Remove(item);
                }
            }

            // Track changes of permission items
            var permissionChanged = false;

            if (rq.IsModified(nameof(rq.Groups)))
            {
                user.PermissionGroups = rq.Groups?.ToList();
                permissionChanged = true;
            }

            if (rq.IsModified(nameof(rq.PermissionIncluded)))
            {
                user.PermissionIncluded = rq.PermissionIncluded?.ToList();
                permissionChanged = true;
            }

            if (rq.IsModified(nameof(rq.PermissionExcluded)))
            {
                user.PermissionExcluded = rq.PermissionExcluded?.ToList();
                permissionChanged = true;
            }

            if (permissionChanged)
            {
                var groupIds = user.PermissionGroups ?? [];
                var includedIds = user.PermissionIncluded ?? [];
                var excludedIds = user.PermissionExcluded ?? [];

                var queryFromGroups = _db.PermissionGroups.AsNoTracking()
                    .Where(g => groupIds.Contains(g.Id))
                    .SelectMany(g => g.Items);

                var queryFromIncluded = _db.PermissionItems.AsNoTracking()
                    .Where(i => includedIds.Contains(i.Id))
                    .Select(i => i.Id);

                var permissionIds = await queryFromGroups
                    .Union(queryFromIncluded)
                    .Where(id => !excludedIds.Contains(id))
                    .Distinct()
                    .ToListAsync(cancellationToken);

                // 清除旧权限
                await _db.PersonPermissionItems.AsNoTracking()
                    .Where(p => p.PersonId == user.Id)
                    .ExecuteDeleteAsync(cancellationToken);

                // 批量插入新权限
                var cacheRecords = permissionIds.Select(id => new PersonPermissionItem
                {
                    PersonId = user.Id,
                    PermissionItemId = id
                }).ToList();

                await _db.BulkInsertAsync(cacheRecords, new BulkConfig
                {
                    BatchSize = 1000
                }, cancellationToken: cancellationToken);
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateUserMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, user.Name),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateUserMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read user data for update
        /// 读取用于更新的用户数据
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<UserUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.User.Edit, cancellationToken))
            {
                return null;
            }

            return await _db.Users(User.OrganizationInt).AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserUpdateReadData
                {
                    Id = u.Id,
                    Name = u.Name,
                    UserRole = u.UserRole,
                    AssignedId = u.AssignedId,
                    ReportTo = u.ReportTo,
                    Expiry = u.Expiry,
                    Status = u.Status,
                    Data = u.Data,
                    Categories = u.CategoryIds,
                    Groups = u.PermissionGroups,
                    PermissionIncluded = u.PermissionIncluded,
                    PermissionExcluded = u.PermissionExcluded,
                    Depts = u.ContactOwners
                        .Where(o => (o.Person.IdentityType & IdentityTypeFlags.Dept) > 0)
                        .Select(o => o.PersonId)
                }).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Read user data for update
        /// 读取用于更新的用户数据
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task UpdateReadAsync(long id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var data = await UpdateReadAsync(id, cancellationToken);
            if (data != null)
            {
                await writer.SerializeAsync(data, MyJsonSerializerContext.Default.UserUpdateReadData);
            }
        }
    }
}