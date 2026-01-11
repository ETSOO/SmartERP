using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Localization;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Product;
using CRM.Server.RQ.Product;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using System.Buffers;
using System.Threading;
using ProductUnit = com.etsoo.CoreFramework.Business.ProductUnit;

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
                    if (!assetQty.HasValue)
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
            var orgId = User.OrganizationInt;

            if (!await _commonService.HasPermissionAsync((short)Permissions.Product.Add, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var assetQtyResult = await ValidateAssetQtyAsync(orgId, rq.UnitId, rq.AssetQty, cancellationToken);
            if (!assetQtyResult.Ok)
            {
                return assetQtyResult;
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

            // Product
            var product = new Product
            {
                CoreOrganizationId = orgId,
                Name = rq.Name,
                ForeignName = rq.ForeignName,
                CategoryIds = rq.Categories?.ToList(),
                Description = rq.Description,
                ForeignDescription = rq.ForeignDescription,
                UnitId = unitId,
                MinQty = rq.MinQty,
                StepQty = rq.StepQty,
                CapQty = rq.CapQty,
                AssetQty = rq.AssetQty,
                AssignedId = assignedId,
                Status = rq.Status ?? EntityStatus.Normal,
                Usage = rq.Usage ?? ProductUsage.FinishedProduct,
                Scope = rq.Scope ?? ProductScope.Public,
                InventoryWay = rq.InventoryWay ?? ProductInventoryWay.None,
                QueryKeyword = queryKeyword
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
                        q = q.Where(p => p.CategoryIds != null && _db.ProductCategories.Any(c => p.CategoryIds.Contains(c.Id)
                            && c.ParentIds != null && c.ParentIds.Contains(rq.CategoryIdAll.Value))
                        );
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
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, a => a.Name, a => a.ForeignName, a => a.Description);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.Name, $"%{keyword}%")
                            || (ou.QueryKeyword != null && EF.Functions.ILike(ou.QueryKeyword, $"%{keyword}%"))
                            || (ou.ForeignName != null && EF.Functions.ILike(ou.ForeignName, $"%{keyword}%"))
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
                Categories = _db.PersonCategories.Where(c => c.CoreOrganizationId == orgId && p.CategoryIds != null && p.CategoryIds.Contains(c.Id)).OrderBy(t => p.CategoryIds!.IndexOf(t.Id)).Select(c => new CategoryItem { Id = c.Id, Names = c.Names }).ToList()
            })
            .ToArrayAsync(cancellationToken);
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
                    ForeignName = u.ForeignName,
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

            if (rq.IsModified(nameof(rq.ForeignName)))
            {
                product.ForeignName = rq.ForeignName;
            }

            if (rq.IsModified(nameof(rq.ForeignDescription)))
            {
                product.ForeignDescription = rq.ForeignDescription;
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
                product.AssetQty = rq.AssetQty;
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

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                product.Status = rq.Status.Value;
            }

            if (rq.IsModified(nameof(rq.Categories)))
            {
                product.CategoryIds = rq.Categories?.ToList();
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
                    ForeignName = p.ForeignName,
                    ForeignDescription = p.ForeignDescription,
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
                entity.ForeignName = unit.ForeignName;
                entity.BaseUnit = unit.BaseUnit;
                entity.OrderIndex = (short)(index + 2);

                index++;
            }

            await _db.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}