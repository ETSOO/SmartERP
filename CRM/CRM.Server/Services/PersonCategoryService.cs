using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.PersonCategory;
using CRM.Server.RQ.PersonCategory;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Person category service
    /// 人员分类服务
    /// </summary>
    public class PersonCategoryService : SEUserService, IPersonCategoryService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public PersonCategoryService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PersonCategoryService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "person_category", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(PersonCategoryCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;
            var parentId = rq.ParentId;

            // No same name
            var sameTitle = await _db.PersonCategories(orgId).AsNoTracking()
                .Where(p => (parentId == null || p.ParentId == parentId) && p.Names[p.Names.Count() - 1] == rq.Name)
                .AnyAsync(cancellationToken);

            if (sameTitle)
            {
                return ApplicationErrors.ItemExists.AsResult(nameof(rq.Name));
            }

            // Validate the parent category
            List<string> names;
            if (parentId.HasValue)
            {
                var parent = await _db.PersonCategories(orgId).AsNoTracking()
                    .Where(c => c.Id == parentId.Value)
                    .Select(c => c.Names)
                    .FirstOrDefaultAsync(cancellationToken);

                if (parent == null)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.ParentId));
                }

                names = parent;
                names.Add(rq.Name);
            }
            else
            {
                names = [rq.Name];
            }

            var category = new PersonCategory
            {
                CoreOrganizationId = orgId,
                IdentityType = rq.IdentityType,
                ParentId = parentId,
                Names = names,
                Data = rq.Data,
                OrderIndex = rq.OrderIndex ?? 0,
            };

            // Add
            _db.PersonCategories.Add(category);

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(category.Id);
        }

        private IQueryable<PersonCategory> CreateQuery(PersonCategoryListRQ rq, Func<IQueryable<PersonCategory>, IQueryable<PersonCategory>>? filters = null)
        {
            var query = _db.PersonCategories(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (c) => c.Id, null, (q) =>
                {
                    if (rq.IdentityType.HasValue)
                    {
                        q = q.Where(c => (c.IdentityType & rq.IdentityType.Value) > 0);
                    }

                    if (rq.ParentId.HasValue)
                    {
                        if (rq.ParentId == 0)
                        {
                            q = q.Where(c => c.ParentId == null);
                        }
                        else
                        {
                            q = q.Where(c => c.ParentId == rq.ParentId.Value);
                        }
                    }

                    if (rq.Keyword?.Length > 0)
                    {
                        var keyword = rq.Keyword;
                        q = q.Where(c => c.Names.Any(name => EF.Functions.ILike(name, $"%{keyword}%")));
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
        /// List person category
        /// 人员分类列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task ListAsync(PersonCategoryListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq)
                .OrderBy(c => c.OrderIndex)
                .ThenBy(c => c.Names)
                .Select(c => new PersonCategoryListData
                {
                    Id = c.Id,
                    Name = string.Join(" -> ", c.Names)
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Merge
        /// 合并
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> MergeAsync(MergeRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            // Validate
            // Two ids are unique and have the same identity type
            var categories = await _db.PersonCategories(orgId).AsNoTracking()
                .Where(c => c.Id == rq.SourceId || c.Id == rq.TargetId)
                .Select(c => new { c.Id, c.IdentityType, c.Names })
                .ToArrayAsync(cancellationToken);

            if (categories.Length != 2 || (categories[0].IdentityType & categories[1].IdentityType) == 0)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.TargetId));
            }

            var source = categories.First(c => c.Id == rq.SourceId);
            var target = categories.First(c => c.Id == rq.TargetId);

            var nextItems = source.Names.Count + 1;

            // Replace the source id with the target id
            await _db.Database.ExecuteSqlAsync($"""
                WITH RECURSIVE descendants AS (
                    SELECT "id" FROM "person_category" WHERE "id" = {source.Id}
                        UNION ALL
                    SELECT c."id" FROM "person_category" c
                        INNER JOIN descendants d ON c."parent_id" = d."id"
                )
                UPDATE "person_category" t
                    SET "names" = {target.Names} || "names"[{nextItems}:],
                        "parent_id" = {target.Id}
                FROM descendants d
                WHERE t."id" = d."id" AND t."id" <> {source.Id};

                UPDATE "person"
                    SET "category_ids" = array_replace("category_ids", {source.Id}, {target.Id})
                WHERE "org_id" = {orgId} AND ("identity_type" & {source.IdentityType}) > 0;
            """, cancellationToken);

            // Delete the source category
            if (rq.DeleteSource is true)
            {
                await _db.PersonCategories(orgId).AsNoTracking()
                    .Where(c => c.Id == rq.SourceId)
                    .ExecuteDeleteAsync(cancellationToken);
            }

            return ActionResult.Succeed(rq.TargetId);
        }

        /// <summary>
        /// Query person category
        /// 查询人员分类
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<PersonCategoryQueryData[]> QueryAsync(PersonCategoryQueryRQ rq, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq)
                .OrderBy(c => c.IdentityType)
                .ThenBy(c => c.OrderIndex)
                .ThenBy(c => c.Names)
                .Select(c => new PersonCategoryQueryData
                {
                    Id = c.Id,
                    Names = c.Names,
                    IdentityType = c.IdentityType,
                    Creation = c.Creation
                }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Sort
        /// 排序
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<int> SortAsync(Dictionary<int, short> rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return -1;
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var ids = rq.Keys.ToArray();
            var indices = rq.Values.ToArray();

#pragma warning disable EF1002 // No risk of vulnerability to SQL injection.
            return await _db.Database.ExecuteSqlRawAsync($"""
                UPDATE "person_category"
                    SET "order_index" = t."sorder_index"
                FROM (VALUES {string.Join(", ", ids.Select((id, i) => $"({id}, {indices[i]})"))}) AS t("sid", "sorder_index")
                WHERE "core_organization_id" = {orgId} AND "id" = t."sid";
            """, cancellationToken);
#pragma warning restore EF1002 // No risk of vulnerability to SQL injection.
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(PersonCategoryUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var category = await _db.PersonCategories(orgId)
                .Where(c => c.Id == rq.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (category == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Original names
            string[] originalNames = [.. category.Names];

            var parentId = rq.ParentId;
            List<string>? parent = null;

            if (parentId.HasValue)
            {
                parent = await _db.PersonCategories(orgId).AsNoTracking()
                    .Where(c => c.Id == parentId.Value)
                    .Select(c => c.Names)
                    .FirstOrDefaultAsync(cancellationToken);

                if (parent == null)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.ParentId));
                }

                category.Names = [.. parent, category.Names.Last()];
            }

            if (rq.IsModified(nameof(rq.IdentityType)) && rq.IdentityType.HasValue)
            {
                category.IdentityType = rq.IdentityType.Value;
            }

            if (rq.IsModified(nameof(rq.ParentId)))
            {
                category.ParentId = parentId;
                if (parentId == null)
                {
                    category.Names = [category.Names.Last()];
                }
            }

            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                category.Names = parent == null ? [rq.Name] : [.. parent, rq.Name];
            }

            if (rq.IsModified(nameof(rq.OrderIndex)) && rq.OrderIndex.HasValue)
            {
                category.OrderIndex = rq.OrderIndex.Value;
            }

            if (rq.IsModified(nameof(rq.Data)))
            {
                category.Data = rq.Data;
            }

            // Dynamic SQL update
            if (!originalNames.SequenceEqual(category.Names))
            {
                var nextItems = originalNames.Length + 1;
                await _db.Database.ExecuteSqlAsync($"""
                    WITH RECURSIVE descendants AS (
                        SELECT "id" FROM "person_category" WHERE "id" = {rq.Id}
                            UNION ALL
                        SELECT c."id" FROM "person_category" c
                            INNER JOIN descendants d ON c."parent_id" = d."id"
                    )
                    UPDATE "person_category" t
                        SET "names" = {category.Names} || "names"[{nextItems}:]
                    FROM descendants d
                    WHERE t."id" = d."id" AND t."id" <> {rq.Id};
                """, cancellationToken);
            }

            // Changes
            // var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read for update
        /// 读取用于更新的数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<PersonCategoryUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return null;
            }

            return await _db.PersonCategories(User.OrganizationInt).AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new PersonCategoryUpdateReadData
                {
                    Id = c.Id,
                    IdentityType = c.IdentityType,
                    ParentId = c.ParentId,
                    Names = c.Names,
                    Data = c.Data
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
