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

        private IQueryable<OrderLine> CreateQuery(OrderLineListRQ rq, Func<IQueryable<OrderLine>, IQueryable<OrderLine>>? filters = null)
        {
            var orgId = User.OrganizationInt;

            return _db.OrderLines.AsNoTracking()
                .Where(p => p.Order.CoreOrganizationId == orgId)
                .QueryEtsoo(rq, p => p.Id, p => p.Status, q =>
                {
                    if (rq.OrderId.HasValue)
                    {
                        q = q.Where(p => p.OrderId == rq.OrderId.Value);
                    }

                    if (rq.ProductId.HasValue)
                    {
                        q = q.Where(p => p.ProductId == rq.ProductId.Value);
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

            var orgId = User.OrganizationInt;

            var orderId = await _db.OrderLines
                .Where(q => q.Id == id && q.Order.CoreOrganizationId == orgId
                            && q.Order.Status < EntityStatus.Inactivated
                            && q.Status < EntityStatus.Inactivated
                            && (isManage || q.Order.UserId == User.Oid))
                .Select(q => q.OrderId)
                .FirstOrDefaultAsync(cancellationToken);

            if (orderId < 1)
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
        /// Query order line
        /// 查询订单行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<OrderLineQueryData[]> QueryAsync(OrderLineQueryRQ rq, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq, (q) =>
            {
                if(rq.StartTimeStart.HasValue)
                {
                    q = q.Where(p => p.StartTime >= rq.StartTimeStart.Value);
                }

                if (rq.StartTimeEnd.HasValue)
                {
                    q = q.Where(p => p.StartTime <= rq.StartTimeEnd.Value);
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
        /// Start to execute the order line
        /// 开始执行订单行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> StartAsync(OrderLineStartRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await _orderService.CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var userId = User.Oid;
            var id = rq.Id;
            var initStart = rq.InitStart ?? false;

            var result = await _db.OrderLines
                .Where(p => p.Id == id
                          && p.Order.CoreOrganizationId == orgId
                          && (isManage || p.Order.UserId == User.Oid)
                          && p.Order.Status < EntityStatus.Inactivated
                          && p.Status < EntityStatus.Inactivated)
                .ExecuteUpdateAsync(p => p.SetProperty(ol => ol.StartTime, ol => initStart ? ol.StartTime ?? DateTimeOffset.Now : ol.StartTime)
                                        .SetProperty(ol => ol.UserId, ol => ol.UserId ?? userId), cancellationToken);

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
                          && (isManage || p.Order.UserId == User.Oid)
                          && p.Order.Status < EntityStatus.Inactivated
                          && p.Status < EntityStatus.Inactivated)
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
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.Edit, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.OrderLines.AsNoTracking()
                .Where(p => p.Id == id && p.Order.CoreOrganizationId == orgId)
                .Select(p => new OrderLineUpdateReadData
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    ProductId = p.ProductId,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Qty = p.Qty,
                    StartTime = p.StartTime,
                    EndTime = p.EndTime,
                    Data = p.Data,
                    Status = p.Status
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
