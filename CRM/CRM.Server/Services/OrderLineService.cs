using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.OrderLine;
using CRM.Server.RQ.OrderLine;
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
    /// Order line service
    /// 订单行服务
    /// </summary>
    public class OrderLineService : SEUserService, IOrderLineService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IProductService _productService;
        readonly IOrderService _orderService;

        public OrderLineService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<OrderLineService> logger,
            ICommonService commonService,
            IProductService productService,
            IOrderService orderService
        )
            : base(app, userAccessor.UserSafe, "order_line", logger)
        {
            _db = db;
            _commonService = commonService;
            _productService = productService;
            _orderService = orderService;
        }

        private IQueryable<OrderLine> CreateQuery(OrderLineListRQ rq, Func<IQueryable<OrderLine>, IQueryable<OrderLine>>? filters = null, bool isOrder = true)
        {
            var orgId = User.OrganizationInt;

            return _db.OrderLines.AsNoTracking()
                .Where(p => p.Order.CoreOrganizationId == orgId)
                .QueryEtsoo(rq, p => p.Id, p => p.Status, q =>
                {
                    if (isOrder)
                    {
                        q = q.Where(p => p.Order.IsOrder);
                    }

                    if (rq.OrderId.HasValue)
                    {
                        q = q.Where(p => p.OrderId == rq.OrderId.Value);
                    }

                    if (rq.SupplierId.HasValue)
                    {
                        q = q.Where(p => p.SupplierId == rq.SupplierId.Value);
                    }

                    if (rq.ProductId.HasValue)
                    {
                        q = q.Where(p => p.ProductId == rq.ProductId.Value);
                    }

                    if (rq.AssetId.HasValue)
                    {
                        q = q.Where(p => p.AssetId == rq.AssetId.Value);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;
                        q = q.Where(ou => EF.Functions.ILike(ou.Title, $"%{keyword}%")
                            || (ou.Description != null && EF.Functions.ILike(ou.Description, $"%{keyword}%")));
                    }

                    if (filters != null)
                    {
                        q = filters(q);
                    }

                    return q;
                });
        }

        /// <summary>
        /// Complete the order line
        /// 完成订单行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CompleteAsync(OrderLineCompleteRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var permissions = await _commonService.HasPermissionsAsync([
                (short)Permissions.Order.Execute,
                (short)Permissions.Order.Manage
            ], cancellationToken);

            var isExecute = permissions[0];
            var isManage = permissions[1];

            if (!isExecute)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var userId = User.Oid;
            var id = rq.Id;
            var assetId = rq.AssetId;
            var supplierId = rq.SupplierId;
            var costPrice = rq.CostPrice;
            var now = DateTimeOffset.UtcNow;

            var line = await _db.OrderLines
                .Where(p => p.Id == id
                          && p.Order.CoreOrganizationId == orgId
                          && p.Order.IsOrder
                          && p.Order.Status < EntityStatus.Inactivated
                          && p.Status < EntityStatus.Inactivated
                          && (isManage || p.Order.UserId == userId || p.UserId == userId)
                          && (p.StartTime < now || (p.AssetQty > 0 && p.AssetId == null)))
                .Select(a => new
                {
                    a.Order.BuyerId,
                    a.ProductId,
                    a.Qty,
                    a.AssetId,
                    a.AssetQty,
                    a.UserId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (line == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Supplier
            if (supplierId.HasValue)
            {
                var hasSupplier = await _db.Suppliers(orgId).Where(s => s.Id == supplierId.Value)
                    .AnyAsync(cancellationToken);

                if (!hasSupplier)
                {
                    return ApplicationErrors.ItemNotExists.AsResult(nameof(rq.SupplierId));
                }
            }

            // Asset
            if (assetId.HasValue)
            {
                var hasAsset = await _db.PersonAssets.Where(a => a.Id == assetId.Value && a.PersonId == line.BuyerId)
                    .AnyAsync(cancellationToken);

                if (!hasAsset)
                {
                    return ApplicationErrors.ItemNotExists.AsResult(nameof(rq.AssetId));
                }
            }
            else if (line.AssetId == null && line.AssetQty > 0 && !assetId.HasValue)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.AssetId));
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (assetId.HasValue)
                {
                    var assetResult = await _commonService.SyncAssetAsync(line.BuyerId, assetId.Value, line.AssetQty, line.Qty, cancellationToken);

                    if (!assetResult.Ok)
                    {
                        await transaction.RollbackAsync(cancellationToken);

                        return assetResult;
                    }
                }

                await _db.OrderLines
                                .Where(p => p.Id == id)
                                .ExecuteUpdateAsync(p => p.SetProperty(ol => ol.EndTime, now)
                                                        .SetProperty(ol => ol.AssetId, ol => ol.AssetId ?? assetId)
                                                        .SetProperty(ol => ol.SupplierId, ol => supplierId ?? ol.SupplierId)
                                                        .SetProperty(ol => ol.CostPrice, ol => costPrice ?? ol.CostPrice)
                                                        .SetProperty(ol => ol.UserId, ol => ol.UserId ?? userId)
                                                        .SetProperty(ol => ol.Status, EntityStatus.Completed), cancellationToken)
                                ;

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Rollback
                await transaction.RollbackAsync(cancellationToken);

                // Log and return the result
                return LogException(ex);
            }

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(OrderLineCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await _orderService.CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var orderId = rq.OrderId;

            var order = await _db.Orders(orgId)
                .Where(o => o.Id == orderId && (isManage || o.UserId == User.Oid) && o.Status < EntityStatus.Inactivated)
                .Select(o => new
                {
                    CustomerId = o.BuyerId,
                    o.Currency,
                    o.Culture
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.OrderId));
            }

            var productRQ = new QueryForSaleRQ
            {
                CustomerId = order.CustomerId,
                Currency = order.Currency,
                Culture = order.Culture,
                Ids = [rq.ProductId]
            };

            var products = await _productService.QueryForSaleAsync(productRQ, cancellationToken);
            if (products == null || products.Length != 1)
            {
                return ApplicationErrors.DataOutdated.AsResult();
            }

            var product = products[0];

            var qty = rq.Qty;

            var qtyResult = _productService.ValidateQty(product, qty);
            if (qtyResult != null)
            {
                return qtyResult;
            }

            var price = _productService.GetSalePrice(product);

            if (rq.Price.HasValue && rq.Price.Value != price)
            {
                return ApplicationErrors.DataOutdated.AsResult(nameof(rq.Price));
            }

            var sale = new PromotionCodeLine
            {
                Price = price,
                Qty = qty
            };

            var amount = price * qty;

            var (linePromotions, lineResult) = _productService.ValidatePromotions(rq.Promotions, product.Promotions, amount, sale);
            if (!lineResult.Ok)
            {
                return lineResult;
            }

            var title = rq.Title ?? product.Name;
            var discount = linePromotions?.Sum(p => p.Amount) ?? 0;
            var netAmount = amount - discount;

            var orderLine = new OrderLine
            {
                OrderId = orderId,
                ProductId = rq.ProductId,
                Title = title,
                Description = rq.Description,
                OriginalPrice = product.RetailPrice,
                CostPrice = product.CostPrice ?? 0,
                Price = price,
                Qty = qty,
                AssetQty = product.AssetQty.GetValueOrDefault(),
                Amount = netAmount,
                Discount = discount,
                StartTime = rq.StartTime,
                EndTime = rq.EndTime,
                Data = rq.Data,
                Status = rq.Status ?? EntityStatus.Normal
            };

            _db.OrderLines.Add(orderLine);

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await _db.SaveChangesAsync(cancellationToken);

                var result = await _orderService.RecalculateAsync(orderId, false, cancellationToken);
                if (!result.Ok)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return result;
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Rollback
                await transaction.RollbackAsync(cancellationToken);

                // Log and return the result
                return LogException(ex);
            }

            return ActionResult.Succeed(orderLine.Id);
        }

        /// <summary>
        /// Delete
        /// 删除
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await _orderService.CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            if (!isEdit)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var orderId = await _db.OrderLines
                .Where(q => q.Id == id && q.Order.CoreOrganizationId == orgId
                            && q.Order.IsOrder
                            && q.Order.Status < EntityStatus.Inactivated
                            && q.Status < EntityStatus.Inactivated
                            && (isManage || q.Order.UserId == User.Oid)
                            && q.AssetId == null)
                .Select(q => q.OrderId)
                .FirstOrDefaultAsync(cancellationToken);

            if (orderId == 0)
            {
                return ApplicationErrors.NoId.AsResult(nameof(id));
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await _db.OrderLines.Where(q => q.Id == id).ExecuteDeleteAsync(cancellationToken);

                var result = await _orderService.RecalculateAsync(orderId, false, cancellationToken);

                if (!result.Ok)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return result;
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Rollback
                await transaction.RollbackAsync(cancellationToken);

                // Log and return the result
                return LogException(ex);
            }

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// List order line JSON data
        /// 订单行列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task ListAsync(OrderLineListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq)
                .OrderByDescending(c => c.Creation)
                .ThenBy(c => c.Id)
                .Select(p => new OrderLineListData
                {
                    Id = p.Id,
                    Title = p.Title,
                    Qty = p.Qty
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query all order lines
        /// 查询所有订单行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrderLineQueryAllData[]> QueryAllAsync(OrderLineQueryAllRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.Order.Query, (short)Permissions.Order.Manage], cancellationToken);
            var isQuery = permissions[0];
            var isManage = permissions[1];
            if (!isQuery)
            {
                return [];
            }

            if (!isManage)
            {
                // Limit to current user
                rq.UserId = User.Oid;
            }

            return await CreateQuery(rq, (q) =>
            {
                if (rq.UserId.HasValue)
                {
                    q = q.Where(p => p.UserId == rq.UserId.Value || p.Order.UserId == rq.UserId.Value);
                }

                if (rq.Source?.Length is > 1)
                {
                    q = q.Where(p => p.Order.Source == rq.Source.ToUpper());
                }

                if (rq.CustomerId.HasValue)
                {
                    q = q.Where(p => p.Order.BuyerId == rq.CustomerId.Value);
                }

                if (rq.QtyStart.HasValue)
                {
                    q = q.Where(p => p.Qty >= rq.QtyStart.Value);
                }

                if (rq.CreationStart.HasValue)
                {
                    q = q.Where(p => p.Creation >= rq.CreationStart.Value);
                }

                if (rq.CreationEnd.HasValue)
                {
                    q = q.Where(p => p.Creation < rq.CreationEnd.Value);
                }

                if (rq.StartTimeStart.HasValue)
                {
                    q = q.Where(p => p.StartTime >= rq.StartTimeStart.Value);
                }

                if (rq.StartTimeEnd.HasValue)
                {
                    q = q.Where(p => p.StartTime < rq.StartTimeEnd.Value);
                }

                return q;
            })
            .TagWith(nameof(QueryAllAsync))
            .Select(p => new OrderLineQueryAllData
            {
                Id = p.Id,
                Source = p.Order.Source,
                Customer = p.Order.Buyer.Name,
                CustomerId = p.Order.BuyerId,
                OrderId = p.OrderId,
                ProductId = p.ProductId,
                Title = p.Title,
                Description = p.Description,
                Currency = p.Order.Currency,
                Price = p.Price,
                Qty = p.Qty,
                Amount = p.Amount,
                Discount = p.Discount,
                StartTime = p.StartTime,
                EndTime = p.EndTime,
                Status = p.Status,
                Creation = p.Creation
            })
            .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Query asset order line
        /// 查询资产订单行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrderLineQueryAssetData[]> QueryAssetAsync(OrderLineQueryAssetRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Org.Manage, cancellationToken))
            {
                return [];
            }

            return await CreateQuery(rq, (q) =>
            {
                if (rq.UserId.HasValue)
                {
                    q = q.Where(p => p.UserId == rq.UserId.Value || p.Order.UserId == rq.UserId.Value);
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
            }, false)
            .TagWith(nameof(QueryAssetAsync))
            .Select(p => new OrderLineQueryAssetData
            {
                Id = p.Id,
                IsOrder = p.Order.IsOrder,
                Title = p.Title,
                CostPrice = p.CostPrice,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier == null ? null : p.Supplier.Name,
                Price = p.Price,
                Qty = p.Qty,
                AssetQty = p.AssetQty,
                Status = p.Status,
                Creation = p.Creation
            })
            .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Query order line
        /// 查询订单行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrderLineQueryData[]> QueryAsync(OrderLineQueryRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.Order.View, (short)Permissions.Order.Manage], cancellationToken);
            var isView = permissions[0];
            var isManage = permissions[1];
            if (!isView)
            {
                return [];
            }

            return await CreateQuery(rq, (q) =>
            {
                if (!isManage)
                {
                    var userId = User.Oid;
                    q = q.Where(p => p.UserId == userId || p.Order.UserId == userId);
                }

                if (rq.QtyStart.HasValue)
                {
                    q = q.Where(p => p.Qty >= rq.QtyStart.Value);
                }

                return q;
            })
            .TagWith(nameof(QueryAsync))
            .Select(p => new OrderLineQueryData
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                Qty = p.Qty,
                Amount = p.Amount,
                Discount = p.Discount,
                StartTime = p.StartTime,
                EndTime = p.EndTime,
                Status = p.Status,
                Creation = p.Creation
            })
            .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Read data for view
        /// 读取用于浏览的数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrderLineViewData?> ReadAsync(long id, CancellationToken cancellationToken = default)
        {
            // Permission check
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.Order.View, (short)Permissions.Order.Execute, (short)Permissions.Order.Manage], cancellationToken);
            var isView = permissions[0];
            var isExecute = permissions[1];
            var isManage = permissions[2];
            if (!isView && !isExecute)
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            var data = await _db.OrderLines.AsNoTracking()
                .Where(p => p.Id == id && p.Order.CoreOrganizationId == orgId && p.Order.IsOrder)
                .Select(p => new OrderLineViewData
                {
                    Id = p.Id,
                    OrderTitle = p.Order.Title,
                    OrderId = p.OrderId,
                    Currency = p.Order.Currency,
                    ProductName = p.Product.Name,
                    ProductId = p.ProductId,
                    Title = p.Title,
                    Description = p.Description,
                    OriginalPrice = p.OriginalPrice,
                    CostPrice = p.CostPrice,
                    Price = p.Price,
                    Qty = p.Qty,
                    QtyDelivered = p.QtyDelivered,
                    AssetQty = p.AssetQty,
                    Amount = p.Amount,
                    Discount = p.Discount,
                    Promotions = p.Promotions,
                    StartTime = p.StartTime,
                    EndTime = p.EndTime,
                    UserName = (p.User == null ? null : p.User.Name),
                    UserId = p.UserId,
                    OrderUserId = p.Order.UserId,
                    CustomerId = p.Order.BuyerId,
                    AssetId = p.AssetId,
                    AssetSn = (p.Asset == null ? null : p.Asset.Sn),
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier == null ? null : p.Supplier.Name,
                    Modifiers = p.Product.Modifiers,
                    Data = p.Data,
                    Status = p.Status,
                    OrderStatus = p.Order.Status,
                    Creation = p.Creation
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data != null)
            {
                var userId = User.Oid;
                var now = DateTimeOffset.UtcNow;

                data.IsStartable = isExecute
                    && data.Status < EntityStatus.Inactivated
                    && data.OrderStatus < EntityStatus.Inactivated
                    && (data.UserId == null || data.StartTime == null);

                data.IsCompletable = isExecute
                        && data.Status < EntityStatus.Inactivated
                        && data.OrderStatus < EntityStatus.Inactivated
                        && (isManage || data.OrderUserId == userId || data.UserId == userId)
                        && (data.StartTime < now || (data.AssetQty > 0 && data.AssetId == null));

                data.IsRestorable = isExecute
                        && (isManage || data.OrderUserId == userId || data.UserId == userId)
                        && data.Status != EntityStatus.Completed && data.Status != EntityStatus.Normal;
            }

            return data;
        }

        /// <summary>
        /// Rollback the order line
        /// 回滚订单行
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> RollbackAsync(long id, CancellationToken cancellationToken = default)
        {
            // Permission check
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.Order.Execute, (short)Permissions.Order.Manage], cancellationToken);
            var isExecute = permissions[0];
            var isManage = permissions[1];
            if (!isExecute)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var userId = User.Oid;

            var line = await _db.OrderLines
                .Where(p => p.Id == id
                          && p.Order.CoreOrganizationId == orgId
                          && p.Order.IsOrder
                          && (isManage || p.Order.UserId == userId || p.UserId == userId)
                          && p.Status != EntityStatus.Normal && p.Status != EntityStatus.Completed
                ).Select(p => new
                {
                    p.Order.BuyerId,
                    p.ProductId,
                    p.Qty,
                    p.AssetId,
                    p.AssetQty
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (line == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (line.AssetId.HasValue)
                {
                    var assetResult = await _commonService.SyncAssetAsync(line.BuyerId, line.AssetId.Value, -line.AssetQty, line.Qty, cancellationToken);
                    if (!assetResult.Ok)
                    {
                        await transaction.RollbackAsync(cancellationToken);

                        return assetResult;
                    }
                }

                await _db.OrderLines
                    .Where(p => p.Id == id)
                    .ExecuteUpdateAsync(p => p.SetProperty(ol => ol.StartTime, (DateTimeOffset?)null)
                                            .SetProperty(ol => ol.EndTime, (DateTimeOffset?)null)
                                            .SetProperty(ol => ol.UserId, (long?)null)
                                            .SetProperty(ol => ol.AssetId, (int?)null)
                                            .SetProperty(ol => ol.SupplierId, (int?)null)
                                            .SetProperty(ol => ol.Status, EntityStatus.Normal), cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Rollback
                await transaction.RollbackAsync(cancellationToken);

                // Log and return the result
                return LogException(ex);
            }

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Start to execute the order line
        /// 开始执行订单行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> StartAsync(OrderLineStartRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.Order.Execute, (short)Permissions.Order.Manage], cancellationToken);
            var isExecute = permissions[0];
            var isManage = permissions[1];
            if (!isExecute)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var userId = User.Oid;
            var id = rq.Id;
            var initStart = rq.InitStart ?? false;
            var now = DateTimeOffset.UtcNow;

            var result = await _db.OrderLines
                .Where(p => p.Id == id
                          && p.Order.CoreOrganizationId == orgId
                          && p.Order.IsOrder
                          && p.Order.Status < EntityStatus.Inactivated
                          && p.Status < EntityStatus.Inactivated)
                .ExecuteUpdateAsync(p => p.SetProperty(ol => ol.StartTime, ol => initStart || ol.StartTime == null ? now : ol.StartTime)
                                        .SetProperty(ol => ol.UserId, ol => (isManage || ol.Order.UserId == userId) ? rq.UserId ?? ol.UserId ?? userId : ol.UserId ?? userId)
                                        .SetProperty(ol => ol.Status, ol => ol.Status == EntityStatus.Normal ? EntityStatus.Doing : ol.Status), cancellationToken);

            return result > 0 ? ActionResult.Succeed(id) : ApplicationErrors.NoId.AsResult();
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(OrderLineUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await _orderService.CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var orderLine = await _db.OrderLines
                .Where(p => p.Id == rq.Id
                          && p.Order.CoreOrganizationId == orgId
                          && p.Order.IsOrder
                          && (isManage || p.Order.UserId == User.Oid)
                          && p.Order.Status < EntityStatus.Inactivated)
                .FirstOrDefaultAsync(cancellationToken);

            if (orderLine == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var amountUpdated = false;

            if (rq.IsModified(nameof(rq.Price)) && rq.Price.HasValue)
            {
                orderLine.Price = rq.Price.Value;
                amountUpdated = true;
            }

            if (rq.IsModified(nameof(rq.Qty)) && rq.Qty.HasValue)
            {
                orderLine.Qty = rq.Qty.Value;
                amountUpdated = true;
            }

            if (amountUpdated)
            {
                orderLine.Amount = orderLine.Price * orderLine.Qty - orderLine.Discount;
            }

            if (rq.IsModified(nameof(rq.Title)) && !string.IsNullOrEmpty(rq.Title))
            {
                orderLine.Title = rq.Title;
            }

            if (rq.IsModified(nameof(rq.OriginalPrice)) && rq.OriginalPrice.HasValue)
            {
                orderLine.OriginalPrice = rq.OriginalPrice.Value;
            }

            if (rq.IsModified(nameof(rq.CostPrice)) && rq.CostPrice.HasValue)
            {
                orderLine.CostPrice = rq.CostPrice.Value;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                orderLine.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.StartTime)))
            {
                orderLine.StartTime = rq.StartTime;
            }

            if (rq.IsModified(nameof(rq.EndTime)))
            {
                orderLine.EndTime = rq.EndTime;
            }

            if (rq.IsModified(nameof(rq.Data)))
            {
                orderLine.Data = rq.Data;
            }

            if (rq.IsModified(nameof(rq.SupplierId)))
            {
                if (rq.SupplierId.HasValue)
                {
                    // 并不限制SupplierId必须是供应商编号，以增强系统灵活性
                    var supplierExists = await _db.Persons(orgId).Where(s => s.Id == rq.SupplierId.Value).AnyAsync(cancellationToken);
                    if (!supplierExists)
                    {
                        return ApplicationErrors.NoId.AsResult(nameof(rq.SupplierId));
                    }
                }

                orderLine.SupplierId = rq.SupplierId;
            }

            if (rq.IsModified(nameof(rq.UserId)) && rq.UserId.HasValue)
            {
                var userExists = await _db.Users(orgId).Where(u => u.Id == rq.UserId.Value).AnyAsync(cancellationToken);
                if (!userExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.UserId));
                }

                orderLine.UserId = rq.UserId.Value;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                orderLine.Status = rq.Status.Value;
            }

            if (amountUpdated)
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    await _db.SaveChangesAsync(cancellationToken);

                    var result = await _orderService.RecalculateAsync(orderLine.OrderId, false, cancellationToken);

                    if (!result.Ok)
                    {
                        await transaction.RollbackAsync(cancellationToken);

                        return result;
                    }

                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    // Rollback
                    await transaction.RollbackAsync(cancellationToken);

                    // Log and return the result
                    return LogException(ex);
                }
            }
            else
            {
                await _db.SaveChangesAsync(cancellationToken);
            }

            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read data for update
        /// 读取用于更新的数据
        /// </summary>
        /// <param name="id">Order line id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrderLineUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await _orderService.CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return null;
            }

            var orgId = User.OrganizationInt;
            var userId = User.Oid;

            return await _db.OrderLines.AsNoTracking()
                .Where(p => p.Id == id && p.Order.CoreOrganizationId == orgId && p.Order.IsOrder)
                .Select(p => new OrderLineUpdateReadData
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    Currency = p.Order.Currency,
                    ProductId = p.ProductId,
                    Title = p.Title,
                    Description = p.Description,
                    OriginalPrice = p.OriginalPrice,
                    CostPrice = p.CostPrice,
                    Price = p.Price,
                    Qty = p.Qty,
                    StartTime = p.StartTime,
                    EndTime = p.EndTime,
                    SupplierId = p.SupplierId,
                    UserId = p.UserId,
                    Modifiers = p.Product.Modifiers,
                    Data = p.Data,
                    Status = p.Status,
                    IsDeletable = p.Status < EntityStatus.Inactivated && p.Order.Status < EntityStatus.Inactivated && p.AssetId == null && (isManage || p.Order.UserId == userId)
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
