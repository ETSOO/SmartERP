using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.SmartERP;
using CRM.Server.Dto.StockSite;
using CRM.Server.RQ.StockSite;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;

namespace CRM.Server.Services
{
    /// <summary>
    /// Stock site service
    /// 库存点服务
    /// </summary>
    public class StockSiteService : SEUserService, IStockSiteService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public StockSiteService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<StockSiteService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "stock_site", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        /// <summary>
        /// Query purchase line
        /// 查询采购行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<StockSiteQueryData[]> QueryAsync(StockSiteQueryRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Query, cancellationToken))
            {
                return [];
            }

            var orgId = User.OrganizationInt;

            // Query
            var query = _db.StockSites.AsNoTracking()
                .Where(s => s.LocationId == rq.LocationId && s.Product.CoreOrganizationId == orgId)
                .QueryEtsoo(rq, (s) => s.Id, (s) => s.Product.Status, (q) =>
                {
                    if (rq.ProductId.HasValue)
                    {
                        q = q.Where(s => s.ProductId == rq.ProductId.Value);
                    }

                    if (rq.RefreshTimeStart.HasValue)
                    {
                        q = q.Where(s => s.RefreshTime >= rq.RefreshTimeStart.Value);
                    }

                    if (rq.RefreshTimeEnd.HasValue)
                    {
                        q = q.Where(s => s.RefreshTime < rq.RefreshTimeEnd.Value);
                    }

                    return q;
                });

            return await query
                .Select(s => new StockSiteQueryData
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = s.Product.Name,
                    Qty = s.Qty,
                    RefreshTime = s.RefreshTime
                })
                .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// View product
        /// 浏览产品
        /// </summary>
        /// <param name="id">Product id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<StockSiteViewProductData[]> ViewProductAsync(int id, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Query, cancellationToken))
            {
                return [];
            }

            var orgId = User.OrganizationInt;

            return await _db.StockSites.AsNoTracking()
                .Where(s => s.ProductId == id && s.Product.CoreOrganizationId == orgId)
                .Select(s => new StockSiteViewProductData
                {
                    Id = s.Id,
                    LocationId = s.LocationId,
                    LocationName = s.Location == null ? null : s.Location.Name,
                    Qty = s.Qty,
                    RefreshTime = s.RefreshTime
                })
                .ToArrayAsync(cancellationToken);
        }
    }
}
