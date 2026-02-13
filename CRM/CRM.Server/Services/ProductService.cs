using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using com.etsoo.WebUtils.Attributes;
using CRM.Server.Dto.Product;
using CRM.Server.Dto.System;
using CRM.Server.RQ.Product;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Product service
    /// 产品服务
    /// </summary>
    public class ProductService : SEUserService, IProductService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public ProductService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<ProductService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "product", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        private async ValueTask<ActionResult> ValidateAssetQtyAsync(int orgId, int? unitId, int? assetQty, CancellationToken cancellationToken)
        {
            if (unitId.HasValue)
            {
                var bu = await _db.ProductUnits.AsNoTracking()
                    .Where(u => u.Id == unitId && (u.CoreOrganizationId == null || u.CoreOrganizationId == orgId))
                    .Select(u => new { u.BaseUnit })
                    .FirstOrDefaultAsync(cancellationToken);

                if (bu == null)
                {
                    return ApplicationErrors.NoValidData.AsResult("UnitId");
                }
                else if (Constants.IsAssetUnit(bu.BaseUnit))
                {
                    if (!assetQty.HasValue || assetQty.Value < 0)
                    {
                        return ApplicationErrors.NoValidData.AsResult("AssetQty");
                    }
                }
                else if (assetQty.HasValue)
                {
                    return ApplicationErrors.NoValidData.AsResult("AssetQty");
                }
            }
            else if (assetQty.HasValue)
            {
                return ApplicationErrors.NoValidData.AsResult("AssetQty");
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(ProductCreateRQ rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Add, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var assetQtyResult = await ValidateAssetQtyAsync(orgId, rq.UnitId, rq.AssetQty, cancellationToken);
            if (!assetQtyResult.Ok)
            {
                return assetQtyResult;
            }

            // Categories
            var categoryIds = rq.Categories;
            var (result, ids) = await _commonService.ValidateCategoriesAsync(categoryIds, orgId, cancellationToken);
            if (!result.Ok)
            {
                return result;
            }

            // Duplicate test
            // Name
            if (await _db.Products(orgId).AnyAsync(p => p.Name.ToUpper() == rq.Name.ToUpper(), cancellationToken))
            {
                return ApplicationErrors.ItemExists.AsResult("Name");
            }

            // Assigned id
            var assignedId = rq.AssignedId?.ToUpper();
            if (!string.IsNullOrEmpty(assignedId)
                && await _db.Products(orgId).AnyAsync(p => p.AssignedId != null && p.AssignedId == assignedId, cancellationToken))
            {
                return ApplicationErrors.ItemExists.AsResult("AssignedId");
            }

            var queryKeyword = rq.QueryKeyword;
            if (string.IsNullOrEmpty(queryKeyword))
            {
                queryKeyword = ChineseUtils.GetPinyin(rq.Name).ToInitials();
            }

            var unitId = rq.UnitId ?? 1;

            var assetQty = rq.AssetQty;

            // When 0 means the unit is an asset unit but don't want to manage it as an asset
            if (assetQty < 1) assetQty = null;

            // Product
            var product = new Product
            {
                CoreOrganizationId = orgId,
                Name = rq.Name,
                CategoryIds = categoryIds?.ToList(),
                CategoryIdsAll = ids?.ToList(),
                Description = rq.Description,
                UnitId = unitId,
                MinQty = rq.MinQty,
                StepQty = rq.StepQty,
                CapQty = rq.CapQty,
                AssetQty = assetQty,
                AssignedId = assignedId,
                Status = rq.Status ?? EntityStatus.Normal,
                Usage = rq.Usage ?? ProductUsage.FinishedProduct,
                Scope = rq.Scope ?? ProductScope.Public,
                InventoryWay = rq.InventoryWay ?? ProductInventoryWay.None,
                QueryKeyword = queryKeyword,
                TaxRate = rq.TaxRate,
                IntroductionUrl = rq.IntroductionUrl
            };

            if (rq.Tags?.Any() is true)
            {
                var tagIds = await _commonService.AddTagsAsync(FeatureTagKind.Product, rq.Tags, cancellationToken);
                product.Tags = [.. tagIds];
            }

            if (rq.Price != null)
            {
                product.Prices =
                [
                    new ProductPrice
                    {
                        Currency = rq.Price.Currency,
                        RetailPrice = rq.Price.RetailPrice,
                        PromotionPrice = rq.Price.PromotionPrice,
                        ChannelPrice = rq.Price.ChannelPrice,
                        CostPrice = rq.Price.CostPrice
                    }
                ];
            }

            _db.Products.Add(product);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            return ActionResult.Succeed(product.Id);
        }

        private IQueryable<Product> CreateQuery(ProductListRQ rq, Func<IQueryable<Product>, IQueryable<Product>>? filters = null)
        {
            var query = _db.Products(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (p) => p.Id, (p) => p.Status, (q) =>
                {
                    if (rq.TagId != null)
                    {
                        q = q.Where(p => p.Tags != null && p.Tags.Contains(rq.TagId.Value));
                    }

                    if (rq.CategoryIdAll.HasValue)
                    {
                        q = q.Where(p => p.CategoryIdsAll != null && p.CategoryIdsAll.Contains(rq.CategoryIdAll.Value));
                    }
                    else if (rq.CategoryId.HasValue)
                    {
                        q = q.Where(p => p.CategoryIds != null && p.CategoryIds.Contains(rq.CategoryId.Value));
                    }
                    else if (rq.CategoryIds?.Any() is true)
                    {
                        q = q.Where(p => p.CategoryIds != null && rq.CategoryIds.Any(c => p.CategoryIds.Contains(c)));
                    }

                    if (rq.Name?.Length is > 1)
                    {
                        q = q.Where(ou => EF.Functions.ILike(ou.Name, $"%{rq.Name}%"));
                    }

                    if (rq.AssignedIdStart?.Length is > 1)
                    {
                        q = q.Where(ou => ou.AssignedId != null && EF.Functions.ILike(ou.AssignedId, $"{rq.AssignedIdStart}%"));
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, a => a.Name, a => a.Description);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.Name, $"%{keyword}%")
                            || (ou.QueryKeyword != null && EF.Functions.ILike(ou.QueryKeyword, $"%{keyword}%"))
                            || (ou.Description != null && EF.Functions.ILike(ou.Description, $"%{keyword}%"))
                            || (ou.AssignedId != null && EF.Functions.ILike(ou.AssignedId, $"%{keyword}%"))
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
        /// Duplicate test
        /// 重复测试
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<ProductDuplicateTestData[]?> DuplicateTestAsync(ProductDuplicateTestRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            var q = _db.Products(orgId).AsNoTracking();

            var hasFilter = false;

            if (rq.ExcludedId.HasValue)
            {
                q = q.Where(p => p.Id != rq.ExcludedId.Value);
            }

            if (!string.IsNullOrEmpty(rq.Name))
            {
                q = q.Where(p => p.Name.ToLower() == rq.Name.ToLower());
                hasFilter = true;
            }

            if (!string.IsNullOrEmpty(rq.AssignedId))
            {
                q = q.Where(p => p.AssignedId != null && p.AssignedId == rq.AssignedId.ToUpper());
                hasFilter = true;
            }

            if (!hasFilter) return null;

            return await q.Select(p => new ProductDuplicateTestData
            {
                Id = p.Id,
                Name = p.Name,
                AssignedId = p.AssignedId
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// List product JSON data
        /// 产品列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(ProductListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _commonService.UpdateTagAsync(rq, User.OrganizationInt, cancellationToken);

            await CreateQuery(rq)
                .Select(p => new ProductListData
                {
                    Id = p.Id,
                    Name = p.Name,
                    BaseUnit = p.Unit.BaseUnit,
                    AssignedId = p.AssignedId
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query product
        /// 查询产品
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ProductQueryData[]> QueryAsync(ProductQueryRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            var currency = rq.Currency ?? (await _commonService.GetDefaultCurrency(orgId, cancellationToken)) ?? "USD";

            await _commonService.UpdateTagAsync(rq, orgId, cancellationToken);

            return await CreateQuery(rq, q =>
            {
                if (rq.Scope != null)
                {
                    q = q.Where(p => p.Scope == rq.Scope.Value);
                }

                if (rq.UnitId != null)
                {
                    q = q.Where(p => p.UnitId == rq.UnitId.Value);
                }

                return q;
            })
            .TagWith(nameof(QueryAsync))
            .LeftJoin(_db.ProductPrices.Where(pp => pp.Currency == currency), p => p.Id, pp => pp.ProductId, (p, pp) => new ProductQueryData
            {
                Id = p.Id,
                Name = p.Name,
                AssignedId = p.AssignedId,
                AssetQty = p.AssetQty,
                Scope = p.Scope,
                UnitName = p.Unit.Name,
                Status = p.Status,
                Currency = pp == null ? null : pp.Currency,
                RetailPrice = pp == null ? null : pp.RetailPrice,
                PromotionPrice = pp == null ? null : pp.PromotionPrice,
                Categories = _db.ProductCategories.Where(c => c.CoreOrganizationId == orgId && p.CategoryIds != null && p.CategoryIds.Contains(c.Id)).OrderBy(t => p.CategoryIds!.IndexOf(t.Id)).Select(c => new CategoryItem { Id = c.Id, Names = c.Names }).ToList()
            })
            .ToArrayAsync(cancellationToken);
        }

        public async Task QueryForPurchaseAsync()
        {

        }

        /// <summary>
        /// Query product for sale
        /// 查询产品用于销售
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<QueryForSaleData[]> QueryForSaleAsync(QueryForSaleRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            // Products & prices
            var products = await _db.Products(orgId).AsNoTracking()
                .Where(p => p.Status < EntityStatus.Inactivated && p.Scope > ProductScope.None)
                .QueryEtsoo(rq, (p) => p.Id, null, (q) =>
                {
                    if (rq.CategoryIdAll.HasValue)
                    {
                        q = q.Where(p => p.CategoryIdsAll != null && p.CategoryIdsAll.Contains(rq.CategoryIdAll.Value));
                    }
                    else if (rq.CategoryId.HasValue)
                    {
                        q = q.Where(p => p.CategoryIds != null && p.CategoryIds.Contains(rq.CategoryId.Value));
                    }

                    if (rq.AssignedIdStart?.Length is > 1)
                    {
                        q = q.Where(ou => ou.AssignedId != null && EF.Functions.ILike(ou.AssignedId, $"{rq.AssignedIdStart}%"));
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, a => a.Name, a => a.Description);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.Name, $"%{keyword}%")
                            || (ou.QueryKeyword != null && EF.Functions.ILike(ou.QueryKeyword, $"%{keyword}%"))
                            || (ou.Description != null && EF.Functions.ILike(ou.Description, $"%{keyword}%"))
                            );
                        }
                    }

                    return q;
                })
                .Join(_db.ProductPrices.Where(pp => pp.Currency == rq.Currency), p => p.Id, pp => pp.ProductId, (p, pp) => new QueryForSaleData
                {
                    Id = p.Id,
                    Logo = p.Logo,
                    Name = p.Name,
                    Description = p.Description,
                    AssignedId = p.AssignedId,
                    MinQty = p.MinQty,
                    StepQty = p.StepQty,
                    CapQty = p.CapQty,
                    AssetQty = p.AssetQty,
                    Currency = pp.Currency,
                    RetailPrice = pp.RetailPrice,
                    PromotionPrice = pp.PromotionPrice,
                    UnitName = p.Unit.Name,
                    CategoryIds = p.CategoryIds,
                    CategoryIdsAll = p.CategoryIdsAll
                })
                .ToArrayAsync(cancellationToken);

            if (products.Length == 0) return [];

            var productIds = products.Select(p => p.Id).ToArray();
            var allCategoryIds = products.Where(p => p.CategoryIds != null).SelectMany(p => p.CategoryIds!).Distinct();

            var categories = await _db.ProductCategories(orgId)
                .AsNoTracking()
                .Where(c => allCategoryIds.Contains(c.Id))
                .Select(c => new CategoryItemWithParents { Id = c.Id, Names = c.Names, ParentIds = c.ParentIds })
                .ToArrayAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var promotions = await _db.Promotions(orgId)
                .AsNoTracking()
                .Where(pr => pr.Status < EntityStatus.Inactivated
                    && pr.ValidStart <= now
                    && pr.ValidEnd >= now
                    && ((pr.ProductIds != null && pr.ProductIds.Any(pid => productIds.Contains(pid)))
                        || (pr.ProductCategoryIds != null && pr.ProductCategoryIds.Any(cid => allCategoryIds.Contains(cid)))))
                .Select(pr => new
                {
                    pr.ProductIds,
                    pr.ProductCategoryIds,
                    Promotion = new PromotionItem
                    {
                        Id = pr.Id,
                        Code = pr.Code,
                        Title = pr.Title,
                        MinAmount = pr.MinAmount,
                        Discount = pr.Discount,
                        Stackable = pr.Stackable
                    }
                })
                .ToArrayAsync(cancellationToken);

            // Loop products
            foreach (var p in products)
            {
                // Categories
                if (p.CategoryIds != null)
                {
                    p.Categories = categories
                        .Where(c => p.CategoryIds.Contains(c.Id))
                        .OrderBy(c => p.CategoryIds!.ToArray().IndexOf(c.Id));
                }

                p.Promotions = promotions
                    .Where(pr => (pr.ProductIds != null && pr.ProductIds.Contains(p.Id)) || (pr.ProductCategoryIds != null && p.CategoryIdsAll != null && pr.ProductCategoryIds.Intersect(p.CategoryIdsAll).Any()))
                    .Select(pr => pr.Promotion);
            }

            // Translations
            if (!string.IsNullOrEmpty(rq.Culture))
            {
                var cultureIds = productIds.ToDictionary(id => _commonService.GetCultureKey(id, CustomCultureKind.Product), id => id);
                var allKeys = cultureIds.Keys.ToArray();

                var cultures = await _db.FeatureCultures.AsNoTracking()
                    .Where(c => c.CoreOrganizationId == orgId && allKeys.Contains(c.Key) && c.Culture == rq.Culture)
                    .Select(c => new { c.Key, c.Title, c.Description })
                    .ToArrayAsync(cancellationToken);

                foreach (var c in cultures)
                {
                    if (cultureIds.TryGetValue(c.Key, out var productId))
                    {
                        var p = products.FirstOrDefault(p => p.Id == productId);
                        if (p == null) continue;

                        p.Name = c.Title;
                        p.Description = c.Description;
                    }
                }
            }

            // Customer prices
            if (rq.CustomerId.HasValue)
            {
                var cps = await _db.PersonProducts.AsNoTracking()
                    .Where(cp => cp.PersonId == rq.CustomerId.Value
                        && productIds.Contains(cp.ProductId))
                    .Select(cp => new { cp.ProductId, cp.AssignedId, cp.JsonData })
                    .ToArrayAsync(cancellationToken);

                foreach (var cp in cps)
                {
                    var p = products.FirstOrDefault(p => p.Id == cp.ProductId);
                    if (p == null) continue;

                    p.CustomerAssignedId = cp.AssignedId;

                    if (cp.JsonData != null)
                    {
                        if (cp.JsonData.Cultures != null)
                        {
                            var ci = rq.Culture ?? await _commonService.GetDefaultCulture(orgId, cancellationToken);
                            var culture = cp.JsonData.Cultures.FirstOrDefault(c => c.Culture == ci);
                            if (culture != null)
                            {
                                p.CustomerName = culture.Name;
                                p.CustomerDescription = culture.Description;
                            }
                        }

                        if (cp.JsonData.Prices != null)
                        {
                            var price = cp.JsonData.Prices.FirstOrDefault(p => p.Currency == rq.Currency);
                            if (price != null)
                            {
                                p.CustomerRetailPrice = price.RetailPrice;
                            }
                        }
                    }
                }
            }

            return products;
        }

        /// <summary>
        /// Query product unit
        /// 查询产品单位
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<ProductUnitItem[]> QueryUnitAsync(CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            return _db.ProductUnits.AsNoTracking()
                .Where(u => u.CoreOrganizationId == null || u.CoreOrganizationId == orgId)
                .OrderBy(u => u.OrderIndex)
                .Select(u => new ProductUnitItem
                {
                    Id = u.Id,
                    Name = u.Name,
                    BaseUnit = u.BaseUnit,
                    IsSystem = u.CoreOrganizationId == null
                }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(ProductUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var product = await _db.Products(orgId)
                .Include(p => p.Prices)
                .Where(p => p.Id == rq.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.Name)) && !string.IsNullOrEmpty(rq.Name))
            {
                product.Name = rq.Name;
            }

            if (rq.IsModified(nameof(rq.AssignedId)))
            {
                product.AssignedId = rq.AssignedId?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                product.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.UnitId)) && rq.UnitId.HasValue)
            {
                product.UnitId = rq.UnitId.Value;
            }

            if (rq.IsModified(nameof(rq.MinQty)))
            {
                product.MinQty = rq.MinQty;
            }

            if (rq.IsModified(nameof(rq.StepQty)))
            {
                product.StepQty = rq.StepQty;
            }

            if (rq.IsModified(nameof(rq.CapQty)))
            {
                product.CapQty = rq.CapQty;
            }

            if (rq.IsModified(nameof(rq.AssetQty)))
            {
                var assetQty = rq.AssetQty;
                if (assetQty.HasValue && assetQty.Value < 0) assetQty = null;

                product.AssetQty = assetQty;
            }

            if (rq.IsModified(nameof(rq.Usage)) && rq.Usage.HasValue)
            {
                product.Usage = rq.Usage.Value;
            }

            if (rq.IsModified(nameof(rq.Scope)) && rq.Scope.HasValue)
            {
                product.Scope = rq.Scope.Value;
            }

            if (rq.IsModified(nameof(rq.InventoryWay)) && rq.InventoryWay.HasValue)
            {
                product.InventoryWay = rq.InventoryWay.Value;
            }

            if (rq.IsModified(nameof(rq.QueryKeyword)))
            {
                product.QueryKeyword = rq.QueryKeyword;
            }

            if (rq.IsModified(nameof(rq.Price)) && rq.Price != null)
            {
                var price = rq.Price;
                var existingPrice = product.Prices.FirstOrDefault(p => p.Currency == price.Currency);
                if (existingPrice != null)
                {
                    existingPrice.RetailPrice = price.RetailPrice;
                    existingPrice.PromotionPrice = price.PromotionPrice;
                    existingPrice.ChannelPrice = price.ChannelPrice;
                    existingPrice.CostPrice = price.CostPrice;
                }
                else
                {
                    product.Prices.Add(new ProductPrice
                    {
                        Currency = price.Currency,
                        RetailPrice = price.RetailPrice,
                        PromotionPrice = price.PromotionPrice,
                        ChannelPrice = price.ChannelPrice,
                        CostPrice = price.CostPrice
                    });
                }
            }

            if (rq.IsModified(nameof(rq.TaxRate)))
            {
                product.TaxRate = rq.TaxRate;
            }

            if (rq.IsModified(nameof(rq.IntroductionUrl)))
            {
                product.IntroductionUrl = rq.IntroductionUrl;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                product.Status = rq.Status.Value;
            }

            if (rq.IsModified(nameof(rq.Categories)))
            {
                // Categories
                var categoryIds = rq.Categories;
                var (result, ids) = await _commonService.ValidateCategoriesAsync(categoryIds, orgId, cancellationToken);
                if (!result.Ok)
                {
                    return result;
                }

                product.CategoryIds = categoryIds?.ToList();
                product.CategoryIdsAll = ids?.ToList();
            }

            if (rq.IsModified(nameof(rq.Tags)))
            {
                if (rq.Tags?.Any() is true)
                {
                    var tagIds = await _commonService.AddTagsAsync(FeatureTagKind.Product, rq.Tags, cancellationToken);
                    product.Tags = [.. tagIds];
                }
                else
                {
                    product.Tags = null;
                }
            }

            var assetQtyResult = await ValidateAssetQtyAsync(orgId, product.UnitId, product.AssetQty, cancellationToken);
            if (!assetQtyResult.Ok)
            {
                return assetQtyResult;
            }

            // Save
            await _db.SaveChangesAsync(cancellationToken);

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
        public async Task<ProductUpdateReadData?> UpdateReadAsync(int id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Edit, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            var currency = (await _commonService.GetDefaultCurrency(orgId, cancellationToken)) ?? "USD";

            return await _db.Products(orgId).AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProductUpdateReadData
                {
                    Id = p.Id,
                    Name = p.Name,
                    AssignedId = p.AssignedId,
                    Description = p.Description,
                    UnitId = p.UnitId,
                    MinQty = p.MinQty,
                    StepQty = p.StepQty,
                    CapQty = p.CapQty,
                    AssetQty = p.AssetQty,
                    Usage = p.Usage,
                    Scope = p.Scope,
                    InventoryWay = p.InventoryWay,
                    QueryKeyword = p.QueryKeyword,
                    Price = p.Prices.Where(pp => pp.Currency == currency).Select(pp => new ProductPriceItem
                    {
                        Currency = pp.Currency,
                        RetailPrice = pp.RetailPrice,
                        PromotionPrice = pp.PromotionPrice,
                        ChannelPrice = pp.ChannelPrice,
                        CostPrice = pp.CostPrice
                    }).FirstOrDefault(),
                    TaxRate = p.TaxRate,
                    IntroductionUrl = p.IntroductionUrl,
                    Categories = p.CategoryIds,
                    Tags = p.Tags == null ? null : _db.FeatureTags.Where(k => k.CoreOrganizationId == orgId && p.Tags.Contains(k.Id)).OrderByDescending(t => t.Total).ThenBy(t => t.Tag).Select(k => k.Tag).ToList(),
                    Status = p.Status
                }).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Update product unit
        /// 更新产品单位
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<int> UpdateUnitAsync(ProductUnitUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            int result = 0;

            if (rq.RemovedIds?.Any() is true)
            {
                result = await _db.ProductUnits
                    .Where(u => u.CoreOrganizationId == orgId && rq.RemovedIds.Contains(u.Id) && !u.Products.Any())
                    .ExecuteDeleteAsync(cancellationToken);
            }

            short index = 0;
            foreach (var unit in rq.Items)
            {
                PlatformShared.Database.Models.ProductUnit? entity;

                if (unit.Id is > 0)
                {
                    entity = await _db.ProductUnits
                        .Where(u => u.CoreOrganizationId == orgId && u.Id == unit.Id.Value)
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (entity == null) continue;
                }
                else
                {
                    entity = new PlatformShared.Database.Models.ProductUnit
                    {
                        CoreOrganizationId = orgId
                    };
                    _db.ProductUnits.Add(entity);
                }

                entity.Name = unit.Name;
                entity.BaseUnit = unit.BaseUnit;
                entity.OrderIndex = (short)(index + 2);

                index++;
            }

            await _db.SaveChangesAsync(cancellationToken);

            return result;
        }

        /// <summary>
        /// Read data for view
        /// 读取用于浏览的数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ProductViewData?> ReadAsync(int id, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.View, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            var key = _commonService.GetCultureKey(id, CustomCultureKind.Product);

            return await _db.Products(orgId).AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProductViewData
                {
                    Id = p.Id,
                    Name = p.Name,
                    AssignedId = p.AssignedId,
                    Description = p.Description,
                    UnitId = p.UnitId,
                    MinQty = p.MinQty,
                    StepQty = p.StepQty,
                    CapQty = p.CapQty,
                    AssetQty = p.AssetQty,
                    Usage = p.Usage,
                    Scope = p.Scope,
                    InventoryWay = p.InventoryWay,
                    QueryKeyword = p.QueryKeyword,
                    TaxRate = p.TaxRate,
                    Logo = p.Logo,
                    IntroductionUrl = p.IntroductionUrl,
                    Status = p.Status,
                    Creation = p.Creation,
                    Categories = _db.ProductCategories.Where(c => c.CoreOrganizationId == orgId && p.CategoryIds != null && p.CategoryIds.Contains(c.Id)).OrderBy(t => p.CategoryIds!.IndexOf(t.Id)).Select(c => new CategoryItem { Id = c.Id, Names = c.Names }).ToList(),
                    Prices = p.Prices.Select(pp => new ProductPriceItem
                    {
                        Currency = pp.Currency,
                        RetailPrice = pp.RetailPrice,
                        PromotionPrice = pp.PromotionPrice,
                        ChannelPrice = pp.ChannelPrice,
                        CostPrice = pp.CostPrice
                    }).ToList(),
                    Tags = p.Tags == null ? null : _db.FeatureTags.Where(k => k.CoreOrganizationId == orgId && p.Tags.Contains(k.Id)).OrderByDescending(t => t.Total).ThenBy(t => t.Tag).Select(k => k.Tag).ToList(),
                    Cultures = _db.FeatureCultures.Where(c => c.CoreOrganizationId == orgId && c.Key == key)
                                .Select(c => new CustomCultureItem
                                {
                                    Id = c.Id,
                                    Culture = c.Culture,
                                    Title = c.Title,
                                    Description = c.Description,
                                    JsonData = c.JsonData
                                }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Read custom data
        /// 读取自定义数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ProductReadCustomData?> ReadCustomAsync(int id, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Manage, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            var product = await _db.Products(orgId).AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Name,
                    p.Description,
                    p.AssignedId,
                    Prices = p.Prices.Select(pp => new ProductSimplePriceItem
                    {
                        Currency = pp.Currency,
                        RetailPrice = pp.RetailPrice
                    }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                return null;
            }

            var defaultCulture = await _commonService.GetDefaultCulture(orgId, cancellationToken) ?? App.Configuration.Cultures.First();
            var key = _commonService.GetCultureKey(id, CustomCultureKind.Product);

            var cultures = await _db.FeatureCultures.AsNoTracking()
                .Where(c => c.CoreOrganizationId == orgId && c.Key == key)
                .Select(c => new ProductCustomData
                {
                    Culture = c.Culture,
                    Name = c.Title,
                    Description = c.Description
                })
                .ToListAsync(cancellationToken);

            if (!cultures.Any(c => c.Culture == defaultCulture))
            {
                cultures.Add(new ProductCustomData
                {
                    Culture = defaultCulture,
                    Name = product.Name,
                    Description = product.Description
                });
            }

            return new ProductReadCustomData
            {
                Id = id,
                AssignedId = product.AssignedId,
                Cultures = cultures,
                Prices = product.Prices
            };
        }

        /// <summary>
        /// Read price
        /// 读取价格
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="currency">Currency</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ProductPriceItem?> ReadPriceAsync(int id, string currency, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.View, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.ProductPrices.AsNoTracking()
                .Where(pp => pp.Product.CoreOrganizationId == orgId && pp.ProductId == id && pp.Currency == currency)
                .Select(pp => new ProductPriceItem
                {
                    Currency = pp.Currency,
                    RetailPrice = pp.RetailPrice,
                    PromotionPrice = pp.PromotionPrice,
                    ChannelPrice = pp.ChannelPrice,
                    CostPrice = pp.CostPrice
                }).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Update logo
        /// 更新图标
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateLogoAsync(ProductUpdateLogoRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            // Update
            var result = await _db.Products(orgId).Where(p => p.Id == rq.Id && p.CoreOrganizationId == orgId)
                .ExecuteUpdateAsync(p => p.SetProperty(pu => pu.Logo, rq.Url), cancellationToken);

            if (result == 0)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Create upload logo action data
        /// 创建上传图标的动作数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<AppActionData?> UploadLogoActionAsync(int id, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Edit, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            // Validate product
            var hasProduct = await _db.Products(orgId)
                .AsNoTracking()
                .AnyAsync(p => p.Id == id && p.CoreOrganizationId == orgId, cancellationToken);

            if (!hasProduct)
            {
                return null;
            }

            return App.SignAction("Products", id);
        }

        /// <summary>
        /// Update price
        /// 更新价格
        /// </summary>
        /// <param name="id">Product id</param>
        /// <param name="item">Price item</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdatePriceAsync(int id, ProductPriceItem item, CancellationToken cancellationToken = default)
        {
            // Currency
            if (!new CurrencyAttribute().IsValid(item.Currency))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(item.Currency));
            }

            if (!item.Validate())
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(item));
            }

            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            // Validate product
            var hasProduct = await _db.Products(orgId)
                .AsNoTracking()
                .AnyAsync(p => p.Id == id && p.CoreOrganizationId == orgId, cancellationToken);

            if (!hasProduct)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(id));
            }

            var price = await _db.ProductPrices
                .Where(pp => pp.ProductId == id && pp.Currency == item.Currency)
                .FirstOrDefaultAsync(cancellationToken);

            if (price == null)
            {
                price = new ProductPrice
                {
                    ProductId = id,
                    Currency = item.Currency,
                    RetailPrice = item.RetailPrice,
                    PromotionPrice = item.PromotionPrice,
                    ChannelPrice = item.ChannelPrice,
                    CostPrice = item.CostPrice
                };

                _db.ProductPrices.Add(price);
            }
            else
            {
                price.RetailPrice = item.RetailPrice;
                price.PromotionPrice = item.PromotionPrice;
                price.ChannelPrice = item.ChannelPrice;
                price.CostPrice = item.CostPrice;
            }

            await _db.SaveChangesAsync(cancellationToken);

            return ActionResult.Succeed(id);
        }
    }
}