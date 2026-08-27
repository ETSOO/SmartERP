using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using CRM.Server.Application;
using CRM.Server.Dto.Promotion;
using CRM.Server.RQ.Promotion;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;
using System.Text.Json;

namespace CRM.Server.Services
{
    /// <summary>
    /// Promotion Service
    /// 促销服务
    /// </summary>
    public class PromotionService : MyUserService, IPromotionService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public PromotionService(
            MyDbContext db,
            IMyApp app,
            MyAppConfiguration config,
            CurrentUserAccessor userAccessor,
            ILogger<PromotionService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, config, userAccessor.UserSafe, "promotion", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
        }

        private IQueryable<Promotion> CreateQuery(PromotionListRQ rq, Func<IQueryable<Promotion>, IQueryable<Promotion>>? filters = null)
        {
            var orgId = User.OrganizationInt;

            var query = _db.Promotions(orgId).AsNoTracking()
                .QueryEtsoo(rq, (p) => p.Id, (p) => p.Status, (q) =>
                {
                    if (rq.Code.HasValue)
                    {
                        q = q.Where(p => p.Code == rq.Code.Value);
                    }

                    if (!string.IsNullOrEmpty(rq.Currency))
                    {
                        q = q.Where(p => p.Currency == rq.Currency);
                    }

                    if (rq.IsValid.HasValue)
                    {
                        var now = DateTimeOffset.UtcNow;
                        if (rq.IsValid.Value)
                        {
                            q = q.Where(p => p.ValidStart <= now && p.ValidEnd >= now && p.Status < EntityStatus.Inactivated);
                        }
                        else
                        {
                            q = q.Where(p => p.ValidStart > now || p.ValidEnd < now || p.Status >= EntityStatus.Inactivated);
                        }
                    }

                    if (rq.PersonId.HasValue)
                    {
                        var personId = rq.PersonId.Value;
                        q = q.Where(p => (p.PersonIds != null && p.PersonIds.Contains(personId))
                            || (p.PersonCategoryIds != null && _db.Persons(orgId).Where(pt => pt.Id == personId && pt.CategoryIds != null && pt.CategoryIds.Any(ci => p.PersonCategoryIds.Contains(ci))).Any())
                        );
                    }

                    if (rq.ProductId.HasValue)
                    {
                        var productId = rq.ProductId.Value;
                        q = q.Where(p => (p.ProductIds != null && p.ProductIds.Contains(productId))
                            || (p.ProductCategoryIds != null && _db.Products(orgId).Where(pt => pt.Id == productId && pt.CategoryIds != null && pt.CategoryIds.Any(ci => p.ProductCategoryIds.Contains(ci))).Any())
                        );
                    }

                    if (rq.Stackable.HasValue)
                    {
                        q = q.Where(p => p.Stackable == rq.Stackable.Value);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, a => a.Title);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.Title, $"%{keyword}%"));
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
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(PromotionCreateRQ rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            // Check
            var productIds = rq.ProductIds?.ToList();
            if (productIds?.Count is > 0 && await _db.Products(orgId).CountAsync(p => productIds.Contains(p.Id), cancellationToken) != productIds.Count)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.ProductIds));
            }

            var productCategoryIds = rq.ProductCategoryIds?.ToList();
            if (productCategoryIds?.Count is > 0 && await _db.ProductCategories(orgId).CountAsync(p => productCategoryIds.Contains(p.Id), cancellationToken) != productCategoryIds.Count)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.ProductCategoryIds));
            }

            var personIds = rq.PersonIds?.ToList();
            if (personIds?.Count is > 0 && await _db.Persons(orgId).CountAsync(p => personIds.Contains(p.Id), cancellationToken) != personIds.Count)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.PersonIds));
            }

            var personCategoryIds = rq.PersonCategoryIds?.ToList();
            if (personCategoryIds?.Count is > 0 && await _db.PersonCategories(orgId).CountAsync(p => personCategoryIds.Contains(p.Id), cancellationToken) != personCategoryIds.Count)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.PersonCategoryIds));
            }

            var promotion = new Promotion
            {
                CoreOrganizationId = orgId,
                Title = rq.Title,
                Currency = rq.Currency,
                ProductIds = productIds,
                ProductCategoryIds = productCategoryIds,
                PersonIds = personIds,
                PersonCategoryIds = personCategoryIds,
                Code = rq.Code.Value,
                MinAmount = rq.MinAmount,
                Discount = rq.Discount,
                ValidStart = rq.ValidStart,
                ValidEnd = rq.ValidEnd,
                Coupons = rq.Coupons,
                Stackable = rq.Stackable.GetValueOrDefault(true),
                Status = rq.Status ?? EntityStatus.Normal,
                OrderIndex = rq.OrderIndex.GetValueOrDefault()
            };

            _db.Promotions.Add(promotion);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            var id = promotion.Id;

            // Push message
            var message = new CreatePromotionMessage
            {
                Data = User.CreateMessageData(App.AppId, id, promotion.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.PromotionCreateRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CreatePromotionMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// List product JSON data
        /// 产品列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task ListAsync(PromotionListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq)
                .OrderBy(c => c.OrderIndex)
                .ThenBy(c => c.Id)
                .Select(p => new PromotionListData
                {
                    Id = p.Id,
                    Title = p.Title
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query promotion
        /// 查询促销
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<PromotionQueryData[]> QueryAsync(PromotionQueryRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            return CreateQuery(rq, q =>
            {
                if (rq.CouponsAppliedStart.HasValue)
                {
                    q = q.Where(p => p.CouponsApplied >= rq.CouponsAppliedStart.Value);
                }

                if (rq.CouponsAppliedEnd.HasValue)
                {
                    q = q.Where(p => p.CouponsApplied <= rq.CouponsAppliedEnd.Value);
                }

                return q;
            })
            .TagWith(nameof(QueryAsync))
            .Select(p => new PromotionQueryData
            {
                Id = p.Id,
                Title = p.Title,
                Currency = p.Currency,
                MinAmount = p.MinAmount,
                Discount = p.Discount,
                Coupons = p.Coupons,
                CouponsApplied = p.CouponsApplied,
                Status = p.Status,
                Creation = p.Creation
            })
            .ToArrayAsync(cancellationToken);
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
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Manage, cancellationToken))
            {
                return -1;
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var ids = rq.Keys.ToArray();
            var indices = rq.Values.ToArray();

#pragma warning disable EF1002 // No risk of vulnerability to SQL injection.
            var task1 = _db.Database.ExecuteSqlRawAsync($"""
                UPDATE "promotion"
                    SET "order_index" = t."sorder_index"
                FROM (VALUES {string.Join(", ", ids.Select((id, i) => $"({id}, {indices[i]})"))}) AS t("sid", "sorder_index")
                WHERE "core_organization_id" = {orgId} AND "id" = t."sid";
            """, cancellationToken);
#pragma warning restore EF1002 // No risk of vulnerability to SQL injection.

            // Push message
            var message = new SortPromotionMessage
            {
                Data = User.CreateMessageData(App.AppId, 0),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.DictionaryInt32Int16)
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.SortPromotionMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            return task1.Result;
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(PromotionUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var promotion = await _db.Promotions(orgId)
                .Where(p => p.Id == rq.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (promotion == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.Code)) && rq.Code != null)
            {
                promotion.Code = rq.Code.Value;
            }

            if (rq.IsModified(nameof(rq.Title)) && rq.Title != null)
            {
                promotion.Title = rq.Title;
            }

            if (rq.IsModified(nameof(rq.Currency)) && rq.Currency != null)
            {
                promotion.Currency = rq.Currency;
            }

            if (rq.IsModified(nameof(rq.ProductIds)))
            {
                var productIds = rq.ProductIds?.ToList();
                if (productIds?.Count is > 0)
                {
                    if (await _db.Products(orgId).CountAsync(p => productIds.Contains(p.Id), cancellationToken) != productIds.Count)
                    {
                        return ApplicationErrors.NoValidData.AsResult(nameof(rq.ProductIds));
                    }

                    promotion.ProductIds = productIds;
                }
                else
                {
                    promotion.ProductIds = null;
                }
            }

            if (rq.IsModified(nameof(rq.ProductCategoryIds)))
            {
                var productCategoryIds = rq.ProductCategoryIds?.ToList();
                if (productCategoryIds?.Count is > 0)
                {
                    if (await _db.ProductCategories(orgId).CountAsync(p => productCategoryIds.Contains(p.Id), cancellationToken) != productCategoryIds.Count)
                    {
                        return ApplicationErrors.NoValidData.AsResult(nameof(rq.ProductCategoryIds));
                    }

                    promotion.ProductCategoryIds = productCategoryIds;
                }
                else
                {
                    promotion.ProductCategoryIds = null;
                }
            }

            if (rq.IsModified(nameof(rq.PersonIds)))
            {
                var personIds = rq.PersonIds?.ToList();
                if (personIds?.Count is > 0)
                {
                    if (await _db.Persons(orgId).CountAsync(p => personIds.Contains(p.Id), cancellationToken) != personIds.Count)
                    {
                        return ApplicationErrors.NoValidData.AsResult(nameof(rq.PersonIds));
                    }

                    promotion.PersonIds = personIds;
                }
                else
                {
                    promotion.PersonIds = null;
                }
            }

            if (rq.IsModified(nameof(rq.PersonCategoryIds)))
            {
                var personCategoryIds = rq.PersonCategoryIds?.ToList();
                if (personCategoryIds?.Count is > 0)
                {
                    if (await _db.PersonCategories(orgId).CountAsync(p => personCategoryIds.Contains(p.Id), cancellationToken) != personCategoryIds.Count)
                    {
                        return ApplicationErrors.NoValidData.AsResult(nameof(rq.PersonCategoryIds));
                    }

                    promotion.PersonCategoryIds = personCategoryIds;
                }
                else
                {
                    promotion.PersonCategoryIds = null;
                }
            }

            if (rq.IsModified(nameof(rq.MinAmount)) && rq.MinAmount.HasValue)
            {
                promotion.MinAmount = rq.MinAmount.Value;
            }

            if (rq.IsModified(nameof(rq.Discount)) && rq.Discount.HasValue)
            {
                promotion.Discount = rq.Discount.Value;
            }

            if (rq.IsModified(nameof(rq.ValidStart)) && rq.ValidStart.HasValue)
            {
                promotion.ValidStart = rq.ValidStart.Value;
            }

            if (rq.IsModified(nameof(rq.ValidEnd)) && rq.ValidEnd.HasValue)
            {
                promotion.ValidEnd = rq.ValidEnd.Value;
            }

            if (rq.IsModified(nameof(rq.Coupons)))
            {
                promotion.Coupons = rq.Coupons;
            }

            if (rq.IsModified(nameof(rq.Stackable)) && rq.Stackable.HasValue)
            {
                promotion.Stackable = rq.Stackable.Value;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                promotion.Status = rq.Status.Value;
            }

            if (rq.IsModified(nameof(rq.OrderIndex)) && rq.OrderIndex.HasValue)
            {
                promotion.OrderIndex = rq.OrderIndex.Value;
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdatePromotionMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, promotion.Title),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdatePromotionMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read data for update
        /// 读取用于更新的数据
        /// </summary>
        /// <param name="id">Person id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<PromotionUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Manage, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.Promotions(orgId).AsNoTracking()
                 .Where(p => p.Id == id)
                 .Select(p => new PromotionUpdateReadData
                 {
                     Id = p.Id,
                     Code = p.Code,
                     Title = p.Title,
                     Currency = p.Currency,
                     ProductIds = p.ProductIds,
                     ProductCategoryIds = p.ProductCategoryIds,
                     PersonIds = p.PersonIds,
                     PersonCategoryIds = p.PersonCategoryIds,
                     MinAmount = p.MinAmount,
                     Discount = p.Discount,
                     ValidStart = p.ValidStart,
                     ValidEnd = p.ValidEnd,
                     Coupons = p.Coupons,
                     Stackable = p.Stackable,
                     Status = p.Status,
                     OrderIndex = p.OrderIndex
                 }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}