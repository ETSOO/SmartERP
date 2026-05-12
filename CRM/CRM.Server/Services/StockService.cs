using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.Stock;
using CRM.Server.RQ.Stock;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;

namespace CRM.Server.Services
{
    /// <summary>
    /// Stock service
    /// 库存服务
    /// </summary>
    public class StockService : SEUserService, IStockService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public StockService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<StockService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "stock", logger)
        {
            _db = db;
            _commonService = commonService;
        }

        private IQueryable<StockHeader> CreateQuery(StockListRQ rq, Func<IQueryable<StockHeader>, IQueryable<StockHeader>>? filters = null)
        {
            var orgId = User.OrganizationInt;
            var query = _db.Stocks(orgId).AsNoTracking()
                .QueryEtsoo(rq, (s) => s.Id, null, (q) =>
                {
                    if (rq.Kind.HasValue)
                    {
                        q = q.Where(s => s.Kind == rq.Kind.Value);
                    }

                    if (rq.PersonId.HasValue)
                    {
                        q = q.Where(s => s.PersonId == rq.PersonId.Value);
                    }

                    if (rq.LocationFromId.HasValue)
                    {
                        q = q.Where(s => s.LocationFromId == rq.LocationFromId.Value);
                    }

                    if (rq.LocationToId.HasValue)
                    {
                        q = q.Where(s => s.LocationToId == rq.LocationToId.Value);
                    }

                    if (rq.UserId.HasValue)
                    {
                        q = q.Where(s => s.UserId == rq.UserId.Value);
                    }

                    if (rq.OrderId.HasValue)
                    {
                        q = q.Where(s => s.OrderIds != null && s.OrderIds.Contains(rq.OrderId.Value));
                    }

                    var trackingNumber = rq.TrackingNumber?.Trim().ToUpper();
                    if (trackingNumber?.Length is > 2)
                    {
                        q = q.Where(s => s.TrackingNumber == trackingNumber);
                    }

                    if (rq.Intransit.HasValue)
                    {
                        if (rq.Intransit.Value)
                        {
                            q = q.Where(s => s.ReceiptTime == null);
                        }
                        else
                        {
                            q = q.Where(s => s.ReceiptTime != null);
                        }
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, ou => ou.Title, ou => ou.Description);
                        }
                        else
                        {
                            q = q.Where(s => EF.Functions.Like(s.Title, $"%{keyword}%")
                            || (s.Description != null && EF.Functions.Like(s.Description, $"%{keyword}%"))
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
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(StockCreateRQ rq, CancellationToken cancellationToken = default)
        {
            return ActionResult.Success;
        }

        /// <summary>
        /// List person JSON data
        /// 人员列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(StockListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.List, cancellationToken))
            {
                return;
            }

            await CreateQuery(rq)
                .Select(s => new StockListData
                {
                    Id = s.Id,
                    Kind = s.Kind,
                    Title = s.Title
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query person JSON data
        /// 查询人员JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<StockQueryData[]> QueryAsync(StockQueryRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Query, cancellationToken))
            {
                return [];
            }

            return await CreateQuery(rq, (q) =>
            {
                if (rq.TotalQtyStart.HasValue)
                {
                    q = q.Where(p => p.TotalQty >= rq.TotalQtyStart.Value);
                }

                if (rq.TotalQtyEnd.HasValue)
                {
                    q = q.Where(p => p.TotalQty < rq.TotalQtyEnd.Value);
                }

                if (rq.CreationStart.HasValue)
                {
                    q = q.Where(p => p.Creation >= rq.CreationStart.Value);
                }

                if (rq.CreationEnd.HasValue)
                {
                    q = q.Where(p => p.Creation < rq.CreationEnd.Value);
                }

                return q;
            })
            .Select(s => new StockQueryData
            {
                Id = s.Id,
                Kind = s.Kind,
                LocationFromId = s.LocationFromId,
                LocationFrom = s.LocationFrom.Name,
                LocationToId = s.LocationToId,
                LocationTo = s.LocationTo.Name,
                Title = s.Title,
                Description = s.Description,
                PersonId = s.PersonId,
                PersonName = s.Person.Name,
                TrackingNumber = s.TrackingNumber,
                OrderIds = s.OrderIds,
                TotalLines = s.TotalLines,
                TotalQty = s.TotalQty,
                ReceiptTime = s.ReceiptTime,
                Creation = s.Creation
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Receiving stock
        /// 入库
        /// </summary>
        /// <param name="id">Stock ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> ReceiveAsync(long id, bool checkPermission = true, CancellationToken cancellationToken = default)
        {
            if (checkPermission && !await _commonService.HasPermissionAsync((short)Permissions.Inventory.Add, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var stock = _db.Stocks(orgId).AsNoTracking()
                .Where(s => s.Id == id && s.ReceiptTime == null)
                .Select(s => new { })
                .FirstOrDefaultAsync(cancellationToken);

            if (stock == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            return ActionResult.Succeed(id);
        }
    }
}
