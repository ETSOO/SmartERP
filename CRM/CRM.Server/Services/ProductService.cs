using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using CRM.Server.Dto.Product;
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
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, a => a.Name, a => a.Description);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.Name, $"%{keyword}%")
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

            var currency = rq.Currency
                ?? (await _db.SettingCrms
                .AsNoTracking()
                .Where(s => s.Id == orgId)
                .Select(s => s.Currencies.FirstOrDefault())
                .FirstOrDefaultAsync(cancellationToken)) ?? "CNY";

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
                Categories = _db.PersonCategories.Where(c => c.CoreOrganizationId == orgId && p.CategoryIds.Contains(c.Id)).OrderBy(t => p.CategoryIds.IndexOf(t.Id)).Select(c => new CategoryItem { Id = c.Id, Names = c.Names }).ToList()
            })
            .ToArrayAsync(cancellationToken);
        }
    }
}