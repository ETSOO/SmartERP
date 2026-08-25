using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using com.etsoo.WebUtils.Attributes;
using CRM.Server.Application;
using CRM.Server.Dto.Product;
using CRM.Server.Dto.System;
using CRM.Server.RQ.Product;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Product;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using System.Buffers;
using System.Text.Json;

namespace CRM.Server.Services
{
    /// <summary>
    /// Product service
    /// 产品服务
    /// </summary>
    public class ProductService : MyUserService, IProductService
    {
        // Sale scope includes both internal and public sale
        const ProductScope SaleScope = ProductScope.InternalSale | ProductScope.PublicSale;

        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public ProductService(
            MyDbContext db,
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<ProductService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, userAccessor.UserSafe, "product", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
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
        /// Calculate promotions
        /// 计算促销
        /// </summary>
        /// <param name="promotions">Possible promotions</param>
        /// <param name="amount">Total amount</param>
        /// <param name="sale">Sale item</param>
        /// <returns>Result</returns>
        public IEnumerable<PromotionSaleItem> CalculatePromotions(IEnumerable<PromotionItem> promotions, decimal amount, IPromotionCodeLine? sale = null)
        {
            var saleItems = new List<PromotionSaleItem>();

            // Non-stackable items
            var np = promotions.Where(p => !p.Stackable)
                .Select(p => PromotionCode.TryParse<PromotionCode>(p.Code, out var code) ? code.Calculate(p, sale, amount) : null)
                .OrderByDescending(p => p?.Amount).FirstOrDefault();

            if (np != null)
            {
                saleItems.Add(np);
                amount -= np.Amount;
            }

            // Stackable items
            foreach (var p in promotions.Where(p => p.Stackable))
            {
                if (PromotionCode.TryParse<PromotionCode>(p.Code, out var code))
                {
                    var result = code.Calculate(p, sale, amount);
                    if (result != null)
                    {
                        amount -= result.Amount;
                        saleItems.Add(result);
                    }
                }
            }

            return saleItems;
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
            var (result, ids) = await _commonService.ValidateProductCategoriesAsync(categoryIds, orgId, cancellationToken);
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
                Validity = rq.Validity,
                AssignedId = assignedId,
                Status = rq.Status ?? EntityStatus.Normal,
                Usage = rq.Usage ?? ProductUsage.FinishedProduct,
                Scope = rq.Scope ?? SaleScope,
                QueryKeyword = queryKeyword,
                TaxRate = rq.TaxRate,
                IntroductionUrl = rq.IntroductionUrl,
                Data = rq.Data,
                Modifiers = rq.Modifiers
            };

            if (rq.Tags?.Any() is true)
            {
                var tagIds = await _commonService.AddTagsAsync(FeatureTagKind.Product, rq.Tags, cancellationToken);
                product.Tags = [.. tagIds];
            }

            if (rq.Price != null && rq.Price.RetailPrice.HasValue)
            {
                product.Prices =
                [
                    new ProductPrice
                    {
                        Currency = rq.Price.Currency,
                        RetailPrice = rq.Price.RetailPrice.Value,
                        PromotionPrice = rq.Price.PromotionPrice,
                        ChannelPrice = rq.Price.ChannelPrice,
                        CostPrice = rq.Price.CostPrice
                    }
                ];
            }

            _db.Products.Add(product);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            var id = product.Id;

            // Push message
            var message = new CreateProductMessage
            {
                Data = User.CreateMessageData(App.AppId, id, product.Name),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.ProductCreateRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CreateProductMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        private IQueryable<Product> CreateQuery(ProductListRQ rq, Func<IQueryable<Product>, IQueryable<Product>>? filters = null)
        {
            var query = _db.Products(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (p) => p.Id, (p) => p.Status, (q) =>
                {
                    if (rq.Scope.HasValue)
                    {
                        q = q.Where(p => (p.Scope & rq.Scope.Value) > 0);
                    }

                    if (rq.Usage.HasValue)
                    {
                        q = q.Where(p => p.Usage == rq.Usage.Value);
                    }

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
        /// Delete
        /// 删除
        /// </summary>
        /// <param name="id">Product id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Delete, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var product = await _db.Products(orgId).AsNoTracking()
                .Where(p => p.Id == id).Select(p => new { p.Name, HasOrderLines = p.OrderLines.Any() })
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }
            else if (product.HasOrderLines)
            {
                return ApplicationErrors.DeleteReferencedData.AsResult("Order");
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Price
                var task1 = _db.ProductPrices.AsNoTracking()
                    .Where(pp => pp.ProductId == id)
                    .ExecuteDeleteAsync(cancellationToken);

                // Bom
                var task2 = _db.ProductBoms.AsNoTracking()
                    .Where(pb => pb.ParentId == id)
                    .ExecuteDeleteAsync(cancellationToken);

                await Task.WhenAll(task1, task2);

                // Product itself
                await _db.Products(orgId).AsNoTracking()
                    .Where(p => p.Id == id)
                    .ExecuteDeleteAsync(cancellationToken);

                // Commit
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Rollback
                await transaction.RollbackAsync(cancellationToken);

                // Log
                LogException(ex);

                return ApplicationErrors.DeleteReferencedData.AsResult();
            }

            // Push message
            var message = new DeleteProductMessage
            {
                Data = User.CreateMessageData(App.AppId, id, product.Name)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.DeleteProductMessage, cancellationToken);

            return ActionResult.Succeed(id);
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
        /// Edit BOMs
        /// 编辑物料清单
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> EditBomsAsync(ProductEditBomsRQ rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var parentId = rq.ParentId;

            // Check product
            var product = await _db.Products(orgId)
                .Where(p => p.Id == parentId)
                .Select(p => new Product { Id = p.Id, Name = p.Name, Boms = p.Boms })
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.Items.Any())
            {
                // Check BOM items
                var bomProductIds = rq.Items.Select(i => i.ProductId).ToArray();
                var bomProducts = await _db.Products(orgId).AsNoTracking()
                    .Where(p => bomProductIds.Contains(p.Id))
                    .Select(p => new { p.Id, Boms = p.Boms.Select(b => b.ProductId) })
                    .ToArrayAsync(cancellationToken);

                if (bomProductIds.Length != bomProducts.Length)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(rq.Items));
                }

                // Second level check
                var secondLevelBomIds = bomProducts.SelectMany(p => p.Boms).Distinct().ToArray();

                if (secondLevelBomIds.Length > 0)
                {
                    var hasSecondLevelBoms = await _db.Products(orgId).AsNoTracking()
                        .Where(p => secondLevelBomIds.Contains(p.Id) && p.Boms.Any())
                        .AnyAsync(cancellationToken);

                    if (hasSecondLevelBoms)
                    {
                        return ApplicationErrors.InvalidAction.AsResult(nameof(rq.Items));
                    }
                }

                _db.Attach(product);

                product.Boms = [.. rq.Items.Select(item => new ProductBom { ProductId = item.ProductId, Qty = item.Qty })];

                await _db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // Delete
                await _db.ProductBoms.Where(pb => pb.ParentId == parentId).ExecuteDeleteAsync(cancellationToken);
            }

            // Push message
            var message = new ProductEditBomsMessage
            {
                Data = User.CreateMessageData(App.AppId, parentId, product.Name),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.ProductEditBomsRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.ProductEditBomsMessage, cancellationToken);

            return ActionResult.Succeed(parentId);
        }

        /// <summary>
        /// Get sale price
        /// 获取销售价格
        /// </summary>
        /// <param name="product">Product data</param>
        /// <returns>Result</returns>
        public decimal GetSalePrice(QueryForSaleData product)
        {
            var price = product.RetailPrice;

            if (product.PromotionPrice.HasValue && product.PromotionPrice.Value < price)
            {
                price = product.PromotionPrice.Value;
            }

            if (product.CustomerRetailPrice.HasValue && product.CustomerRetailPrice.Value < price)
            {
                price = product.CustomerRetailPrice.Value;
            }

            return price;
        }

        /// <summary>
        /// Get purchase price
        /// 获取采购价格
        /// </summary>
        /// <param name="product">Product data</param>
        /// <returns>Result</returns>
        public decimal? GetPurchasePrice(QueryForPurchaseData product)
        {
            var price = product.CostPrice;

            if (product.SupplierRetailPrice.HasValue && product.SupplierRetailPrice.Value < price)
            {
                price = product.SupplierRetailPrice.Value;
            }

            return price;
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

        /// <summary>
        /// Query product for purchase
        /// 查询产品用于采购
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="checkScope">Check scope or not</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<QueryForPurchaseData[]> QueryForPurchaseAsync(QueryForPurchaseRQ rq, bool checkScope = true, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;
            var supplierId = rq.SupplierId;
            var currency = rq.Currency;

            // Validate the supplier id in the organization
            var supplierData = await _db.Suppliers(orgId).AsNoTracking()
                .Where(s => s.Id == supplierId)
                .Select(s => new { s.Id, s.CategoryIdsAll })
                .FirstOrDefaultAsync(cancellationToken);

            if (supplierData == null)
            {
                return [];
            }

            // Products & prices
            var products = await _db.Products(orgId).AsNoTracking()
                .Where(p => p.Status < EntityStatus.Inactivated)
                .QueryEtsoo(rq, (p) => p.Id, null, (q) =>
                {
                    if (checkScope)
                    {
                        q = q.Where(p => (p.Scope & ProductScope.Purchase) > 0);
                    }

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
                            var format = $"%{keyword}%";
                            q = q.Where(p => EF.Functions.ILike(p.Name, format)
                            || (p.QueryKeyword != null && EF.Functions.ILike(p.QueryKeyword, format))
                            || (p.Description != null && EF.Functions.ILike(p.Description, format))
                            );
                        }
                    }

                    return q;
                })
                .Join(_db.ProductPrices.Where(pp => pp.Currency == currency), p => p.Id, pp => pp.ProductId, (p, pp) => new QueryForPurchaseData
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
                    CostPrice = pp.CostPrice,
                    UnitId = p.UnitId,
                    UnitName = p.Unit.Name,
                    Modifiers = p.Modifiers,
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
                    && pr.Currency == currency
                    && (pr.Coupons == null || pr.Coupons < 1 || pr.CouponsApplied < pr.Coupons)
                    && (pr.PersonIds != null && pr.PersonIds.Contains(supplierId)) // Supplier promotions should be specific to the supplier
                    && ((pr.ProductIds != null && pr.ProductIds.Any(pid => productIds.Contains(pid)))
                        || (pr.ProductCategoryIds != null && pr.ProductCategoryIds.Any(cid => allCategoryIds.Contains(cid))))
                )
                .OrderBy(pr => pr.OrderIndex).ThenBy(pr => pr.Id)
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

            // Translations
            if (!string.IsNullOrEmpty(rq.Culture))
            {
                var cultureIds = productIds.Select(id => _commonService.GetCultureKey(id, CustomCultureKind.Product));
                var categoryIds = allCategoryIds.Select(id => _commonService.GetCultureKey(id, CustomCultureKind.ProductCategory));
                var unitIds = products.Select(p => p.UnitId).Distinct().Select(id => _commonService.GetCultureKey(id, CustomCultureKind.ProductUnit));
                var promotionIds = promotions.Select(pr => pr.Promotion.Id).Select(id => _commonService.GetCultureKey(id, CustomCultureKind.Promotion));

                string[] allKeys = [.. cultureIds, .. categoryIds, .. unitIds, .. promotionIds];

                var cultures = await _db.FeatureCultures.AsNoTracking()
                    .Where(c => c.CoreOrganizationId == orgId && allKeys.Contains(c.Key) && c.Culture == rq.Culture)
                    .Select(c => new { c.Key, c.Title, c.Description })
                    .ToArrayAsync(cancellationToken);

                var cultureItems = cultures.Where(c => cultureIds.Contains(c.Key)).ToDictionary(c => c.Key, c => c);
                var categoryItems = cultures.Where(c => categoryIds.Contains(c.Key)).ToDictionary(c => c.Key, c => c);
                var unitItems = cultures.Where(c => unitIds.Contains(c.Key)).ToDictionary(c => c.Key, c => c);
                var promotionItems = cultures.Where(c => promotionIds.Contains(c.Key)).ToDictionary(c => c.Key, c => c);

                foreach (var p in products)
                {
                    if (cultureItems.Count > 0)
                    {
                        var idKey = _commonService.GetCultureKey(p.Id, CustomCultureKind.Product);
                        if (cultureItems.TryGetValue(idKey, out var culture))
                        {
                            p.Name = culture.Title;
                            if (!string.IsNullOrEmpty(culture.Description)) p.Description = culture.Description;
                        }
                    }

                    if (unitItems.Count > 0)
                    {
                        var unitKey = _commonService.GetCultureKey(p.UnitId, CustomCultureKind.ProductUnit);
                        if (unitItems.TryGetValue(unitKey, out var u))
                        {
                            p.UnitName = u.Title;
                        }
                    }
                }

                if (categoryItems.Count > 0)
                {
                    foreach (var category in categories)
                    {
                        var categoryKey = _commonService.GetCultureKey(category.Id, CustomCultureKind.ProductCategory);
                        var names = category.Names.ToArray();
                        if (categoryItems.TryGetValue(categoryKey, out var c))
                        {
                            // Update the last item
                            if (names.Length > 0)
                            {
                                names[^1] = c.Title;
                            }
                        }

                        if (category.ParentIds != null)
                        {
                            var index = 0;
                            foreach (var parentId in category.ParentIds)
                            {
                                var parentKey = _commonService.GetCultureKey(parentId, CustomCultureKind.ProductCategory);
                                if (categoryItems.TryGetValue(parentKey, out var pc))
                                {
                                    names[index] = pc.Title;
                                }

                                index++;
                            }
                        }

                        // Update
                        category.Names = names;
                    }
                }

                if (promotionItems.Count > 0)
                {
                    foreach (var promotion in promotions)
                    {
                        var promotionKey = _commonService.GetCultureKey(promotion.Promotion.Id, CustomCultureKind.Promotion);
                        if (promotionItems.TryGetValue(promotionKey, out var pr))
                        {
                            promotion.Promotion.Title = pr.Title;
                        }
                    }
                }
            }

            // Custom price
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

            var cps = await _db.PersonProducts.AsNoTracking()
                .Where(cp => cp.PersonId == supplierId
                    && productIds.Contains(cp.ProductId))
                .Select(cp => new { cp.ProductId, cp.AssignedId, cp.JsonData })
                .ToArrayAsync(cancellationToken);

            foreach (var cp in cps)
            {
                var p = products.FirstOrDefault(p => p.Id == cp.ProductId);
                if (p == null) continue;

                p.SupplierAssignedId = cp.AssignedId;

                if (cp.JsonData != null)
                {
                    if (cp.JsonData.Cultures != null)
                    {
                        var ci = rq.Culture ?? await _commonService.GetDefaultCulture(orgId, cancellationToken);
                        var culture = cp.JsonData.Cultures.FirstOrDefault(c => c.Culture == ci);
                        if (culture != null)
                        {
                            p.SupplierName = culture.Name;
                            p.SupplierDescription = culture.Description;
                        }
                    }

                    if (cp.JsonData.Prices != null)
                    {
                        var price = cp.JsonData.Prices.FirstOrDefault(p => p.Currency == rq.Currency);
                        if (price != null && price.RetailPrice.HasValue)
                        {
                            p.SupplierRetailPrice = price.RetailPrice.Value;
                        }
                    }
                }
            }

            return products;
        }

        /// <summary>
        /// Query product for sale
        /// 查询产品用于销售
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="checkScope">Check scope or not</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<QueryForSaleData[]> QueryForSaleAsync(QueryForSaleRQ rq, bool checkScope = true, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;
            var customerId = rq.CustomerId;
            var currency = rq.Currency;

            IEnumerable<int>? customerCategories = null;
            if (customerId.HasValue)
            {
                // Validate the customer id in the organization
                var customerData = await _db.Customers(orgId).AsNoTracking()
                    .Where(c => c.Id == customerId)
                    .Select(c => new { c.Id, c.CategoryIdsAll})
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerData == null)
                {
                    return [];
                }

                customerCategories = customerData.CategoryIdsAll;
            }

            // Products & prices
            var products = await _db.Products(orgId).AsNoTracking()
                .Where(p => p.Status < EntityStatus.Inactivated)
                .QueryEtsoo(rq, (p) => p.Id, null, (q) =>
                {
                    if (checkScope)
                    {
                        q = q.Where(p => (p.Scope & SaleScope) > 0);
                    }

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
                            var format = $"%{keyword}%";
                            q = q.Where(p => EF.Functions.ILike(p.Name, format)
                            || (p.QueryKeyword != null && EF.Functions.ILike(p.QueryKeyword, format))
                            || (p.Description != null && EF.Functions.ILike(p.Description, format))
                            );
                        }
                    }

                    return q;
                })
                .Join(_db.ProductPrices.Where(pp => pp.Currency == currency), p => p.Id, pp => pp.ProductId, (p, pp) => new QueryForSaleData
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
                    CostPrice = pp.CostPrice,
                    UnitId = p.UnitId,
                    Scope = p.Scope,
                    Boms = p.Boms.Select(b => new ProductBomItem
                    {
                        ProductId = b.ProductId,
                        Qty = b.Qty
                    }).ToArray(),
                    UnitName = p.Unit.Name,
                    Modifiers = p.Modifiers,
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
                    && pr.Currency == currency
                    && (pr.Coupons == null || pr.Coupons < 1 || pr.CouponsApplied < pr.Coupons)
                    && ((pr.ProductIds != null && pr.ProductIds.Any(pid => productIds.Contains(pid)))
                        || (pr.ProductCategoryIds != null && pr.ProductCategoryIds.Any(cid => allCategoryIds.Contains(cid))))
                    && ((pr.PersonIds == null && pr.PersonCategoryIds == null) || (pr.PersonIds != null && customerId.HasValue && pr.PersonIds.Contains(customerId.Value)
                        || (pr.PersonCategoryIds != null && customerCategories != null && pr.PersonCategoryIds.Any(pc => customerCategories.Contains(pc)))))
                )
                .OrderBy(pr => pr.OrderIndex).ThenBy(pr => pr.Id)
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

            // Translations
            if (!string.IsNullOrEmpty(rq.Culture))
            {
                var cultureIds = productIds.Select(id => _commonService.GetCultureKey(id, CustomCultureKind.Product));
                var categoryIds = allCategoryIds.Select(id => _commonService.GetCultureKey(id, CustomCultureKind.ProductCategory));
                var unitIds = products.Select(p => p.UnitId).Distinct().Select(id => _commonService.GetCultureKey(id, CustomCultureKind.ProductUnit));
                var promotionIds = promotions.Select(pr => pr.Promotion.Id).Select(id => _commonService.GetCultureKey(id, CustomCultureKind.Promotion));

                string[] allKeys = [..cultureIds, ..categoryIds, ..unitIds, .. promotionIds];

                var cultures = await _db.FeatureCultures.AsNoTracking()
                    .Where(c => c.CoreOrganizationId == orgId && allKeys.Contains(c.Key) && c.Culture == rq.Culture)
                    .Select(c => new { c.Key, c.Title, c.Description })
                    .ToArrayAsync(cancellationToken);

                var cultureItems = cultures.Where(c => cultureIds.Contains(c.Key)).ToDictionary(c => c.Key, c => c);
                var categoryItems = cultures.Where(c => categoryIds.Contains(c.Key)).ToDictionary(c => c.Key, c => c);
                var unitItems = cultures.Where(c => unitIds.Contains(c.Key)).ToDictionary(c => c.Key, c => c);
                var promotionItems = cultures.Where(c => promotionIds.Contains(c.Key)).ToDictionary(c => c.Key, c => c);

                foreach (var p in products)
                {
                    if (cultureItems.Count > 0)
                    {
                        var idKey = _commonService.GetCultureKey(p.Id, CustomCultureKind.Product);
                        if (cultureItems.TryGetValue(idKey, out var culture))
                        {
                            p.Name = culture.Title;
                            if (!string.IsNullOrEmpty(culture.Description)) p.Description = culture.Description;
                        }
                    }

                    if (unitItems.Count > 0)
                    {
                        var unitKey = _commonService.GetCultureKey(p.UnitId, CustomCultureKind.ProductUnit);
                        if (unitItems.TryGetValue(unitKey, out var u))
                        {
                            p.UnitName = u.Title;
                        }
                    }
                }

                if (categoryItems.Count > 0)
                {
                    foreach (var category in categories)
                    {
                        var categoryKey = _commonService.GetCultureKey(category.Id, CustomCultureKind.ProductCategory);
                        var names = category.Names.ToArray();
                        if (categoryItems.TryGetValue(categoryKey, out var c))
                        {
                            // Update the last item
                            if (names.Length > 0)
                            {
                                names[^1] = c.Title;
                            }
                        }

                        if (category.ParentIds != null)
                        {
                            var index = 0;
                            foreach (var parentId in category.ParentIds)
                            {
                                var parentKey = _commonService.GetCultureKey(parentId, CustomCultureKind.ProductCategory);
                                if (categoryItems.TryGetValue(parentKey, out var pc))
                                {
                                    names[index] = pc.Title;
                                }

                                index++;
                            }
                        }

                        // Update
                        category.Names = names;
                    }
                }

                if (promotionItems.Count > 0)
                {
                    foreach (var promotion in promotions)
                    {
                        var promotionKey = _commonService.GetCultureKey(promotion.Promotion.Id, CustomCultureKind.Promotion);
                        if (promotionItems.TryGetValue(promotionKey, out var pr))
                        {
                            promotion.Promotion.Title = pr.Title;
                        }
                    }
                }
            }

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

            // Customer prices
            if (customerId.HasValue)
            {
                var cps = await _db.PersonProducts.AsNoTracking()
                    .Where(cp => cp.PersonId == customerId.Value
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
                            if (price != null && price.RetailPrice.HasValue)
                            {
                                p.CustomerRetailPrice = price.RetailPrice.Value;
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

            if (rq.IsModified(nameof(rq.Validity)))
            {
                product.Validity = rq.Validity;
            }

            if (rq.IsModified(nameof(rq.Usage)) && rq.Usage.HasValue)
            {
                product.Usage = rq.Usage.Value;
            }

            if (rq.IsModified(nameof(rq.Scope)) && rq.Scope.HasValue)
            {
                product.Scope = rq.Scope.Value;
            }

            if (rq.IsModified(nameof(rq.QueryKeyword)))
            {
                product.QueryKeyword = rq.QueryKeyword;
            }

            if (rq.IsModified(nameof(rq.Price)) && rq.Price != null && rq.Price.RetailPrice.HasValue)
            {
                var price = rq.Price;
                var existingPrice = product.Prices.FirstOrDefault(p => p.Currency == price.Currency);
                if (existingPrice != null)
                {
                    existingPrice.RetailPrice = price.RetailPrice.Value;
                    existingPrice.PromotionPrice = price.PromotionPrice;
                    existingPrice.ChannelPrice = price.ChannelPrice;
                    existingPrice.CostPrice = price.CostPrice;
                }
                else
                {
                    product.Prices.Add(new ProductPrice
                    {
                        Currency = price.Currency,
                        RetailPrice = price.RetailPrice.Value,
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

            if (rq.IsModified(nameof(rq.Data)))
            {
                product.Data = rq.Data;
            }

            if (rq.IsModified(nameof(rq.Modifiers)))
            {
                product.Modifiers = rq.Modifiers;
            }

            if (rq.IsModified(nameof(rq.Categories)))
            {
                // Categories
                var categoryIds = rq.Categories;
                var (result, ids) = await _commonService.ValidateProductCategoriesAsync(categoryIds, orgId, cancellationToken);
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

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateProductMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, product.Name),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateProductMessage, cancellationToken);

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
                    Validity = p.Validity,
                    Usage = p.Usage,
                    Scope = p.Scope,
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
                    Status = p.Status,
                    Data = p.Data,
                    Modifiers = p.Modifiers
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

            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateProductUnitMessage
            {
                Data = User.CreateMessageData(App.AppId, orgId),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.ProductUnitUpdateRQ)
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateProductUnitMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

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
                    Unit = p.Unit.Name,
                    MinQty = p.MinQty,
                    StepQty = p.StepQty,
                    CapQty = p.CapQty,
                    AssetQty = p.AssetQty,
                    Validity = p.Validity,
                    Usage = p.Usage,
                    Scope = p.Scope,
                    QueryKeyword = p.QueryKeyword,
                    TaxRate = p.TaxRate,
                    Logo = p.Logo,
                    IntroductionUrl = p.IntroductionUrl,
                    Status = p.Status,
                    Data = p.Data,
                    Creation = p.Creation,
                    Boms = p.Boms.Select(b => new ProductBomNameItem
                    {
                        ProductId = b.ProductId,
                        Qty = b.Qty,
                        Name = b.Product.Name
                    }).ToList(),
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

            // Push message
            var message = new UpdateProductLogoMessage
            {
                Data = User.CreateMessageData(App.AppId, orgId),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.ProductUpdateLogoRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateProductLogoMessage, cancellationToken);

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
            var productName = await _db.Products(orgId)
                .AsNoTracking()
                .Where(p => p.Id == id && p.CoreOrganizationId == orgId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(productName))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(id));
            }

            if (item.RetailPrice.HasValue)
            {
                var price = await _db.ProductPrices
                    .Where(pp => pp.ProductId == id && pp.Currency == item.Currency)
                    .FirstOrDefaultAsync(cancellationToken);

                if (price == null)
                {
                    price = new ProductPrice
                    {
                        ProductId = id,
                        Currency = item.Currency,
                        RetailPrice = item.RetailPrice.Value,
                        PromotionPrice = item.PromotionPrice,
                        ChannelPrice = item.ChannelPrice,
                        CostPrice = item.CostPrice
                    };

                    _db.ProductPrices.Add(price);
                }
                else
                {
                    price.RetailPrice = item.RetailPrice.Value;
                    price.PromotionPrice = item.PromotionPrice;
                    price.ChannelPrice = item.ChannelPrice;
                    price.CostPrice = item.CostPrice;
                }
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateProductPriceMessage
            {
                Data = User.CreateMessageData(App.AppId, id, productName),
                Changes = changes
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateProductPriceMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        IActionResult CreateNoValidDataResult(string field, decimal targetValue, decimal currentValue, string product)
        {
            var result = ApplicationErrors.NoValidData.AsResult(field);
            result.Detail = $"{targetValue}|{currentValue}|{product}";
            return result;
        }

        /// <summary>
        /// Validate promotions
        /// 验证促销
        /// </summary>
        /// <param name="items">Declared promotions</param>
        /// <param name="promotions">Possible promotions</param>
        /// <param name="amount">Total amount</param>
        /// <param name="sale">Sale item</param>
        /// <returns>Result</returns>
        public (IEnumerable<PromotionSaleItem>? saleItems, IActionResult result) ValidatePromotions(IEnumerable<PromotionSaleItemBase>? items, IEnumerable<PromotionItem> promotions, decimal amount, IPromotionCodeLine? sale = null)
        {
            if (items == null)
            {
                return (null, ActionResult.Success);
            }

            var saleItems = CalculatePromotions(promotions, amount, sale);

            // Validate with items (user side trust)
            foreach (var item in items)
            {
                var saleItem = saleItems.FirstOrDefault(s => s.Id == item.Id);
                if (saleItem == null)
                {
                    var result = ApplicationErrors.DataOutdated.AsResult(nameof(item.Id));
                    result.Detail = item.Amount.ToString();
                    return (null, result);
                }
                else if (saleItem.Amount != item.Amount)
                {
                    var result = ApplicationErrors.DataOutdated.AsResult(nameof(item.Amount));
                    result.Detail = $"{saleItem.Title}|{saleItem.Amount}|{item.Amount}";
                    return (null, result);
                }
            }

            return (saleItems, ActionResult.Success);
        }

        /// <summary>
        /// Validate qty
        /// 验证数量
        /// </summary>
        /// <param name="product">Product data</param>
        /// <param name="qty">Qty</param>
        /// <returns>Result</returns>
        public IActionResult? ValidateQty(IProductQtyValidateData product, decimal qty)
        {
            // MinQty / 起订量
            if (product.MinQty.HasValue && qty < product.MinQty.Value)
            {
                return CreateNoValidDataResult(nameof(product.MinQty), product.MinQty.Value, qty, product.Name);
            }

            // StepQty / 最小单位量
            if (product.StepQty.HasValue && qty % product.StepQty.Value != 0)
            {
                return CreateNoValidDataResult(nameof(product.StepQty), product.StepQty.Value, qty, product.Name);
            }

            // CapQty / 购买上限
            if (product.CapQty.HasValue && qty > product.CapQty.Value)
            {
                return CreateNoValidDataResult(nameof(product.CapQty), product.CapQty.Value, qty, product.Name);
            }

            return null;
        }
    }
}