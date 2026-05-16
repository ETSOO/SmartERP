using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using CRM.Server.Dto.Stock;
using CRM.Server.RQ.Stock;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
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

        /// <summary>
        /// Stock assembly
        /// 库存组装
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> AssembleAsync(StockAssembleRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var orgPersonId = User.Pid;

            // Location check
            var locationId = rq.LocationId;
            var hasLocation = await _db.PersonAddresses(orgPersonId).AsNoTracking()
                .Where(a => a.Id == locationId)
                .AnyAsync(cancellationToken);

            if (!hasLocation)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.LocationId));
            }

            // Items check
            var items = rq.Items.ToArray();
            var scope = ProductScope.Inventory & ProductScope.Production;
            var productIds = items.Select(i => i.ProductId).ToArray();
            var products = await _db.Products(orgId).AsNoTracking()
                .Where(p => productIds.Contains(p.Id)
                    && p.Status < EntityStatus.Inactivated
                    && (p.Scope & scope) > 0
                    && p.Boms != null && p.Boms.Count > 0)
                .Select(p => new { p.Id, p.Boms })
                .ToArrayAsync(cancellationToken);

            if (products.Length != productIds.Length)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.Items));
            }

            // Assemble lines
            var assemberItems = products.SelectMany(p => p.Boms.Select(b => new StockItem
            {
                ProductId = b.ProductId,
                Qty = b.Qty * items.First(i => i.ProductId == p.Id).Qty
            })).ToArray();

            var checkResult = await CheckStockItemsAsync(orgId, assemberItems, cancellationToken);
            if (!checkResult.Ok)
            {
                return checkResult;
            }

            // Stock check
            var checkStockResult = await CheckStockAsync(locationId, assemberItems, cancellationToken);
            if (!checkStockResult.Ok)
            {
                return checkStockResult;
            }

            var lines = items.Select(i => new StockLine
            {
                ProductId = i.ProductId,
                Qty = i.Qty,
                LocationId = locationId
            }).ToList();

            var bomLines = assemberItems.Select(a => new StockLine
            {
                ProductId = a.ProductId,
                Qty = -a.Qty,
                LocationId = locationId
            }).ToArray();

            lines.AddRange(bomLines);

            var userId = User.Oid;
            var totalLines = lines.Count;
            var totalQty = lines.Sum(l => Math.Abs(l.Qty));
            var now = DateTimeOffset.UtcNow;

            var stock = new StockHeader
            {
                OrganizationId = orgId,
                Kind = StockKind.Assembly,
                PersonId = orgPersonId,
                LocationFromId = locationId,
                LocationToId = locationId,
                UserId = userId,
                Title = rq.Title,
                Description = rq.Description,
                Creation = now,
                ReceiptTime = now,
                TotalLines = totalLines,
                TotalQty = totalQty,

                Lines = lines
            };

            // Add to database
            _db.StockHeaders.Add(stock);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            // Return result
            return ActionResult.Succeed(stock.Id);
        }

        /// <summary>
        /// Check stock
        /// 检查库存
        /// </summary>
        /// <param name="locationId">Location id</param>
        /// <param name="items">Items</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CheckStockAsync(int locationId, StockItem[] items, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            var ids = items.Select(i => i.ProductId).ToArray();

            var products = await _db.StockSites.AsNoTracking()
                .Where(s => s.LocationId == locationId && ids.Contains(s.ProductId) && s.Product.CoreOrganizationId == orgId)
                .Select(s => new { s.ProductId, s.Qty })
                .ToArrayAsync(cancellationToken);

            var noIds = ids.Where(i => !products.Any(p => p.ProductId == i)).ToArray();
            if (noIds.Length > 0)
            {
                var result = ApplicationErrors.NoId.AsResult(nameof(items));
                result.Data["ids"] = noIds;
                return result;
            }

            var insufficientIds = items.Where(i => products.First(p => p.ProductId == i.ProductId).Qty < i.Qty).Select(i => i.ProductId).ToArray();
            if (insufficientIds.Length > 0)
            {
                var result = LocalAppErrors.InsufficientStock.AsResult(nameof(items));
                result.Data["ids"] = insufficientIds;
                return result;
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// Check stock
        /// 检查库存
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<IActionResult> CheckStockAsync(CheckStockRQ rq, CancellationToken cancellationToken = default)
        {
            return CheckStockItemsAsync(rq.LocationId, [.. rq.Items], cancellationToken);
        }

        private async Task<IActionResult> CheckStockItemsAsync(int orgId, StockItem[] items, CancellationToken cancellationToken)
        {
            var productIds = items.Select(i => i.ProductId).ToArray();

            var products = await _db.Products(orgId).AsNoTracking()
                .Where(p => productIds.Contains(p.Id) && p.Status < EntityStatus.Inactivated && (p.Scope & ProductScope.Inventory) > 0)
                .CountAsync(cancellationToken);

            if (products != productIds.Length)
            {
                return ApplicationErrors.NoId.AsResult(nameof(items));
            }

            return ActionResult.Success;
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

                    if (rq.InTransit.HasValue)
                    {
                        if (rq.InTransit.Value)
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

        private DateTimeOffset GetDeletableDate()
        {
            // Only allow to delete stocks created within 60 days
            return DateTimeOffset.UtcNow.AddDays(-60);
        }

        /// <summary>
        /// Delete
        /// 删除
        /// </summary>
        /// <param name="id">Stock id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Delete, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var validDate = GetDeletableDate();

            var hasStock = await _db.Stocks(orgId).AsNoTracking()
                .Where(s => s.Id == id && s.Creation >= validDate)
                .AnyAsync(cancellationToken);

            if (!hasStock)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Lines
                await _db.StockLines.AsNoTracking()
                    .Where(l => l.StockId == id)
                    .ExecuteDeleteAsync(cancellationToken);

                // Itself
                await _db.Stocks(orgId).AsNoTracking()
                    .Where(s => s.Id == id)
                    .ExecuteDeleteAsync(cancellationToken);

                // Commit
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Rollback
                await transaction.RollbackAsync(cancellationToken);

                // Log
                return LogException(ex);
            }

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Stock loss
        /// 库存报损
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> LoseAsync(StockLoseRQ rq,  CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var orgPersonId = User.Pid;

            // Location check
            var locationId = rq.LocationId;
            var hasLocation = await _db.PersonAddresses(orgPersonId).AsNoTracking()
                .Where(a => a.Id == locationId)
                .AnyAsync(cancellationToken);

            if (!hasLocation)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.LocationId));
            }

            // Items check
            var items = rq.Items.ToArray();
            var checkResult = await CheckStockItemsAsync(orgId, items, cancellationToken);
            if (!checkResult.Ok)
            {
                return checkResult;
            }

            // Stock check
            var checkStockResult = await CheckStockAsync(locationId, items, cancellationToken);
            if (!checkStockResult.Ok)
            {
                return checkStockResult;
            }

            var lines = items.Select(i => new StockLine
            {
                ProductId = i.ProductId,
                Qty = -i.Qty,
                LocationId = locationId
            }).ToArray();

            var userId = User.Oid;
            var totalLines = lines.Length;
            var totalQty = lines.Sum(l => Math.Abs(l.Qty));
            var now = DateTimeOffset.UtcNow;

            var stock = new StockHeader
            {
                OrganizationId = orgId,
                Kind = StockKind.Loss,
                PersonId = orgPersonId,
                LocationFromId = locationId,
                LocationToId = locationId,
                UserId = userId,
                Title = rq.Title,
                Description = rq.Description,
                Creation = now,
                ReceiptTime = now,
                TotalLines = totalLines,
                TotalQty = totalQty,

                Lines = lines
            };

            // Add to database
            _db.StockHeaders.Add(stock);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            // Return result
            return ActionResult.Succeed(stock.Id);
        }

        /// <summary>
        /// Init
        /// 初始化
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> InitAsync(StockInitRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var orgPersonId = User.Pid;

            // Location check
            var locationId = rq.LocationId;
            var hasLocation = await _db.PersonAddresses(orgPersonId).AsNoTracking()
                .Where(a => a.Id == locationId)
                .AnyAsync(cancellationToken);

            if (!hasLocation)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.LocationId));
            }

            // Items check
            var items = rq.Items.ToArray();
            var checkResult = await CheckStockItemsAsync(orgId, items, cancellationToken);
            if (!checkResult.Ok)
            {
                return checkResult;
            }

            var lines = items.Select(i => new StockLine
            {
                ProductId = i.ProductId,
                Qty = i.Qty,
                LocationId = locationId
            }).ToArray();

            var userId = User.Oid;
            var totalLines = lines.Length;
            var totalQty = lines.Sum(l => l.Qty);
            var now = DateTimeOffset.UtcNow;

            var stock = new StockHeader
            {
                OrganizationId = orgId,
                Kind = StockKind.Init,
                PersonId = orgPersonId,
                LocationFromId = locationId,
                LocationToId = locationId,
                UserId = userId,
                Title = rq.Title,
                Description = rq.Description,
                Creation = now,
                ReceiptTime = now,
                TotalLines = totalLines,
                TotalQty = totalQty,

                Lines = lines
            };

            // Add to database
            _db.StockHeaders.Add(stock);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            // Return result
            return ActionResult.Succeed(stock.Id);
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
        /// Query lines
        /// 查询行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<StockQueryLinesData[]> QueryLinesAsync(StockQueryLinesRQ rq, CancellationToken cancellationToken)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.View, cancellationToken))
            {
                return [];
            }

            var orgId = User.OrganizationInt;

            return await _db.StockLines.AsNoTracking()
                .Where(sl => sl.StockId == rq.StockId && sl.Stock.OrganizationId == orgId)
                .QueryEtsoo(rq, (p) => p.Id, null, (q) =>
                {
                    if (rq.ProductId.HasValue)
                    {
                        q = q.Where(p => p.ProductId == rq.ProductId.Value);
                    }

                    if (rq.QtyStart.HasValue)
                    {
                        q = q.Where(p => p.Qty >= rq.QtyStart.Value);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;
                        q = q.Where(l => EF.Functions.ILike(l.Product.Name, $"%{keyword}%"));
                    }

                    return q;
                })
                .Select(l => new StockQueryLinesData
                {
                    Id = l.Id,
                    ProductId = l.ProductId,
                    ProductName = l.Product.Name,
                    Qty = l.Qty,
                    OrderLineId = l.OrderLineId
                }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Query product stock data
        /// 查询产品库存数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<StockQueryProductData[]> QueryProductAsync(StockQueryProductRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Query, cancellationToken))
            {
                return [];
            }

            var orgId = User.OrganizationInt;

            var scope = ProductScope.Inventory;
            if (rq.Scope.HasValue)
            {
                scope &= rq.Scope.Value;
            }

            rq.Enabled ??= true;

            var locationId = rq.LocationId;

            // Query
            var query = _db.Products(orgId).AsNoTracking()
                .Where(p => (p.Scope & scope) > 0)
                .QueryEtsoo(rq, (p) => p.Id, (p) => p.Status, (q) =>
                {
                    if (rq.Usage.HasValue)
                    {
                        q = q.Where(p => p.Usage == rq.Usage.Value);
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

                    if (rq.UnitId != null)
                    {
                        q = q.Where(p => p.UnitId == rq.UnitId.Value);
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

                    return q;
                });

            return await query
                .SelectMany(
                    p => p.StockSites
                        .Where(s => s.LocationId == locationId)
                        .DefaultIfEmpty(),
                    (p, s) => new StockQueryProductData
                    {
                        Id = p.Id,
                        Name = p.Name,
                        AssignedId = p.AssignedId,
                        Qty = s == null ? null : s.Qty,
                        UnitName = p.Unit.Name
                    }
                )
                .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Order deliver
        /// 订单发货
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> OrderOutAsync(StockOrderOutRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var orgPersonId = User.Pid;

            var customerId = rq.CustomerId;

            // Locations check
            var locationFromId = rq.LocationFromId;
            var locationToId = rq.LocationToId;

            var hasOrgLocation = await _db.PersonAddresses(orgPersonId).AsNoTracking()
                .Where(a => a.Id == locationFromId)
                .AnyAsync(cancellationToken);

            if (!hasOrgLocation)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.LocationFromId));
            }

            var hasCustomerLocation = await _db.PersonAddresses(customerId).AsNoTracking()
                .Where(a => a.Id == locationToId && a.Person.CoreOrganizationId == orgId)
                .AnyAsync(cancellationToken);

            if (!hasCustomerLocation)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.LocationToId));
            }

            // Items check
            var items = rq.Items.ToArray();

            // Stock check
            var checkStockResult = await CheckStockAsync(locationFromId, items, cancellationToken);
            if (!checkStockResult.Ok)
            {
                return checkStockResult;
            }

            var orders = rq.Orders.ToArray();
            var productIds = new int[items.Length];
            var qtys = new decimal[items.Length];
            var lineIds = new long[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                productIds[i] = items[i].ProductId;
                qtys[i] = items[i].Qty;
                lineIds[i] = items[i].OrderLineId;
            }

            var validProducts = await _db.Database.SqlQuery<int>($@"
                SELECT
                    ol.product_id
                FROM order_line AS ol
                    INNER JOIN order_header AS o
                ON ol.order_id = o.id
                    INNER JOIN unnest(
                        {productIds}::int[],
                        {qtys}::numeric[],
                        {lineIds}::bigint[]
                    ) AS t(product_id, qty, order_line_id)
                ON ol.id = t.order_line_id AND ol.product_id = t.product_id
                WHERE od.id = ANY({orders}::bigint[]) AND od.core_organization_id = {orgId} AND ol.qty - ol.qty_delivered >= t.qty
            ").ToArrayAsync(cancellationToken);

            var noIds = productIds.Where(pid => !validProducts.Contains(pid)).ToArray();
            if (noIds.Length > 0)
            {
                var result = ApplicationErrors.NoId.AsResult(nameof(rq.Items));
                result.Data["ids"] = noIds;
                return result;
            }

            var lines = items.Select(i => new StockLine
            {
                ProductId = i.ProductId,
                Qty = -i.Qty,
                LocationId = locationFromId,
                OrderLineId = i.OrderLineId
            }).ToArray();

            var userId = User.Oid;
            var totalLines = lines.Length;
            var totalQty = -lines.Sum(l => l.Qty);
            var now = DateTimeOffset.UtcNow;

            var stock = new StockHeader
            {
                OrganizationId = orgId,
                Kind = StockKind.Order,
                PersonId = orgPersonId,
                LocationFromId = locationFromId,
                LocationToId = locationToId,
                UserId = userId,
                Title = rq.Title,
                Description = rq.Description,
                TrackingNumber = rq.TrackingNumber?.Trim().ToUpper(),
                ReceiptTime = now, // Future tracking may update this value
                Creation = now,
                TotalLines = totalLines,
                TotalQty = totalQty,

                Lines = lines
            };

            // Add to database
            _db.StockHeaders.Add(stock);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            // Return result
            return ActionResult.Succeed(stock.Id);
        }

        /// <summary>
        /// PO receive
        /// 采购入库
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> POInAsync(StockPOInRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var orgPersonId = User.Pid;

            var supplierId = rq.SupplierId;

            // Locations check
            var locationFromId = rq.LocationFromId;
            var locationToId = rq.LocationToId;

            var hasSupplierLocation = await _db.PersonAddresses(supplierId).AsNoTracking()
                .Where(a => a.Id == locationFromId && a.Person.CoreOrganizationId == orgId)
                .AnyAsync(cancellationToken);

            if (!hasSupplierLocation)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.LocationFromId));
            }

            var hasOrgLocation = await _db.PersonAddresses(orgPersonId).AsNoTracking()
                .Where(a => a.Id == locationToId)
                .AnyAsync(cancellationToken);

            if (!hasOrgLocation)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.LocationToId));
            }

            // Items check
            var items = rq.Items.ToArray();

            // Stock check
            var checkStockResult = await CheckStockAsync(locationFromId, items, cancellationToken);
            if (!checkStockResult.Ok)
            {
                return checkStockResult;
            }

            var pos = rq.POs.ToArray();
            var productIds = new int[items.Length];
            var qtys = new decimal[items.Length];
            var lineIds = new long[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                productIds[i] = items[i].ProductId;
                qtys[i] = items[i].Qty;
                lineIds[i] = items[i].OrderLineId;
            }

            var validProducts = await _db.Database.SqlQuery<int>($@"
                SELECT
                    ol.product_id
                FROM order_line AS ol
                    INNER JOIN order_header AS o
                ON ol.order_id = o.id
                    INNER JOIN unnest(
                        {productIds}::int[],
                        {qtys}::numeric[],
                        {lineIds}::bigint[]
                    ) AS t(product_id, qty, order_line_id)
                ON ol.id = t.order_line_id AND ol.product_id = t.product_id
                WHERE o.id = ANY({pos}::bigint[]) AND o.core_organization_id = {orgId} AND ol.qty - ol.qty_delivered >= t.qty
            ").ToArrayAsync(cancellationToken);

            var noIds = productIds.Where(pid => !validProducts.Contains(pid)).ToArray();
            if (noIds.Length > 0)
            {
                var result = ApplicationErrors.NoId.AsResult(nameof(rq.Items));
                result.Data["ids"] = noIds;
                return result;
            }

            var lines = items.Select(i => new StockLine
            {
                ProductId = i.ProductId,
                Qty = i.Qty,
                LocationId = locationFromId,
                OrderLineId = i.OrderLineId
            }).ToArray();

            var userId = User.Oid;
            var totalLines = lines.Length;
            var totalQty = lines.Sum(l => l.Qty);
            var now = DateTimeOffset.UtcNow;

            var stock = new StockHeader
            {
                OrganizationId = orgId,
                Kind = StockKind.PO,
                PersonId = orgPersonId,
                LocationFromId = locationFromId,
                LocationToId = locationToId,
                UserId = userId,
                Title = rq.Title,
                Description = rq.Description,
                TrackingNumber = rq.TrackingNumber?.Trim().ToUpper(),
                ReceiptTime = now,
                Creation = now,
                TotalLines = totalLines,
                TotalQty = totalQty,

                Lines = lines
            };

            // Add to database
            _db.StockHeaders.Add(stock);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            // Return result
            return ActionResult.Succeed(stock.Id);
        }

        /// <summary>
        /// Read data for view
        /// 读取用于浏览的数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<StockViewData?> ReadAsync(long id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.View, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            var data = await _db.Stocks(orgId).AsNoTracking()
                 .Where(s => s.Id == id)
                 .Select(s => new StockViewData
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
                     Orders = _db.Orders(orgId).AsNoTracking()
                        .Where(o => s.OrderIds != null && s.OrderIds.Contains(o.Id))
                        .Select(o => new IdLabelItem<long>
                        {
                            Id = o.Id,
                            Label = o.Title
                        }).ToArray(),
                     UserId = s.UserId,
                     UserName = s.User.Name,
                     TrackingNumber = s.TrackingNumber,
                     TotalLines = s.TotalLines,
                     TotalQty = s.TotalQty,
                     ReceiptTime = s.ReceiptTime,
                     Creation = s.Creation
                 }).FirstOrDefaultAsync(cancellationToken);

            if(data != null)
            {
                data.IsDeletable = data.Creation >= GetDeletableDate();
            }

            return data;
        }

        /// <summary>
        /// Receiving stock
        /// 入库
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> ReceiveAsync(StockReceiveRQ rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Add, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var id = rq.Id;
            var trackingNumber = rq.TrackingNumber?.Trim().ToUpper();

            var hasStock = await _db.Stocks(orgId).AsNoTracking()
                .Where(s => s.Id == id && s.ReceiptTime == null)
                .AnyAsync(cancellationToken);

            if (!hasStock)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var now = DateTimeOffset.UtcNow;

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await _db.Database.ExecuteSqlAsync($@"
                    INSERT INTO stock_line
                    (
                        stock_id,
                        product_id,
                        location_id,
                        -qty,
                        order_line_id
                    )
                    SELECT
                        {id},
                        product_id,
                        location_id,
                        qty,
                        order_line_id
                    FROM stock_line
                    WHERE stock_id = {id}
                ", cancellationToken);

                await _db.Stocks(orgId).AsNoTracking()
                    .Where(s => s.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.ReceiptTime, now)
                                              .SetProperty(p => p.TotalLines, p => 2 * p.TotalLines)
                                              .SetProperty(p => p.TotalQty, p => 2 * p.TotalQty)
                                              .SetProperty(p => p.TrackingNumber, (p) => trackingNumber ?? p.TrackingNumber), cancellationToken);

                // Commit
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Rollback
                await transaction.RollbackAsync(cancellationToken);

                // Log
                return LogException(ex);
            }

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Stock transfer
        /// 库存调货
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> TransferAsync(StockTransferRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var orgPersonId = User.Pid;

            // Locations check
            var locationFromId = rq.LocationFromId;
            var locationToId = rq.LocationToId;

            var locations = await _db.PersonAddresses(orgPersonId).AsNoTracking()
                .Where(a => a.Id == locationFromId || a.Id == locationToId)
                .CountAsync(cancellationToken);

            if (locations != 2)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.LocationFromId));
            }

            // Items check
            var items = rq.Items.ToArray();
            var checkResult = await CheckStockItemsAsync(orgId, items, cancellationToken);
            if (!checkResult.Ok)
            {
                return checkResult;
            }

            // Stock check
            var checkStockResult = await CheckStockAsync(locationFromId, items, cancellationToken);
            if (!checkStockResult.Ok)
            {
                return checkStockResult;
            }

            var lines = items.Select(i => new StockLine
            {
                ProductId = i.ProductId,
                Qty = -i.Qty,
                LocationId = locationFromId
            }).ToArray();

            var userId = User.Oid;
            var totalLines = lines.Length;
            var totalQty = lines.Sum(l => l.Qty);
            var now = DateTimeOffset.UtcNow;

            var stock = new StockHeader
            {
                OrganizationId = orgId,
                Kind = StockKind.StockTransfer,
                PersonId = orgPersonId,
                LocationFromId = locationFromId,
                LocationToId = locationToId,
                UserId = userId,
                Title = rq.Title,
                Description = rq.Description,
                Creation = now,
                TotalLines = totalLines,
                TotalQty = totalQty,

                Lines = lines
            };

            // Add to database
            _db.StockHeaders.Add(stock);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            // Return result
            return ActionResult.Succeed(stock.Id);
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(StockUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var stock = await _db.Stocks(orgId)
                .Where(p => p.Id == rq.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (stock == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if (rq.IsModified(nameof(rq.Title)) && !string.IsNullOrEmpty(rq.Title))
            {
                stock.Title = rq.Title;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                stock.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.TrackingNumber)))
            {
                stock.TrackingNumber = rq.TrackingNumber?.Trim().ToUpper();
            }

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }
    }
}
