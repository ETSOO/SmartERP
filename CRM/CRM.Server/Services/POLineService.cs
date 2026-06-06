using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Dto.POLine;
using CRM.Server.RQ.POLine;
using CRM.Server.RQ.Product;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.PO;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using System.Buffers;
using System.Text.Json;

namespace CRM.Server.Services
{
    /// <summary>
    /// Purchase order line service
    /// 采购订单行服务
    /// </summary>
    public class POLineService : SEUserService, IPOLineService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IProductService _productService;
        readonly IPOService _poService;
        readonly IQueueService _queueService;

        public POLineService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<OrderLineService> logger,
            ICommonService commonService,
            IProductService productService,
            IPOService poService,
            IQueueService queueService
        )
            : base(app, userAccessor.UserSafe, "po_line", logger)
        {
            _db = db;
            _commonService = commonService;
            _productService = productService;
            _poService = poService;
            _queueService = queueService;
        }

        private IQueryable<OrderLine> CreateQuery(POLineListRQ rq, Func<IQueryable<OrderLine>, IQueryable<OrderLine>>? filters = null)
        {
            var orgId = User.OrganizationInt;

            return _db.OrderLines.AsNoTracking()
                .Where(p => p.Order.CoreOrganizationId == orgId && p.Order.Kind == OrderKind.PO)
                .QueryEtsoo(rq, p => p.Id, p => p.Status, q =>
                {
                    if (rq.POId.HasValue)
                    {
                        q = q.Where(p => p.OrderId == rq.POId.Value);
                    }

                    if (rq.SupplierId.HasValue)
                    {
                        q = q.Where(p => p.Order.SellerId == rq.SupplierId.Value);
                    }

                    if (rq.ProductId.HasValue)
                    {
                        q = q.Where(p => p.ProductId == rq.ProductId.Value);
                    }

                    if (rq.AssetId.HasValue)
                    {
                        q = q.Where(p => p.AssetId == rq.AssetId.Value);
                    }


                    if (rq.HasBomId.HasValue)
                    {
                        if (rq.HasBomId.Value)
                        {
                            q = q.Where(p => p.BomId != null);
                        }
                        else
                        {
                            q = q.Where(p => p.BomId == null);
                        }
                    }

                    if (rq.BomId.HasValue)
                    {
                        q = q.Where(p => p.BomId == rq.BomId.Value);
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
        /// Complete the purchase line
        /// 完成采购行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CompleteAsync(POLineCompleteRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var permissions = await _commonService.HasPermissionsAsync([
                (short)Permissions.PO.Execute,
                (short)Permissions.PO.Manage,
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
            var now = DateTimeOffset.UtcNow;

            var line = await _db.OrderLines
                .Where(p => p.Id == id
                          && p.Order.CoreOrganizationId == orgId
                          && p.Order.Kind == OrderKind.PO
                          && p.Order.Status < EntityStatus.Inactivated
                          && p.Status < EntityStatus.Inactivated
                          && (isManage || p.Order.UserId == userId || p.UserId == userId)
                          && (p.StartTime < now || (p.AssetQty > 0 && p.AssetId == null)))
                .Select(a => new
                {
                    a.Title,
                    a.Order.SellerId,
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
                                                        .SetProperty(ol => ol.SupplierId, line.SellerId)
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

            // Push message
            var message = new CompletePOLineMessage
            {
                Data = User.CreateMessageData(App.AppId, id, line.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.POLineCompleteRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CompletePOLineMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(POLineCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await _poService.CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var poId = rq.POId;

            var po = await _db.POs(orgId)
                .Where(o => o.Id == poId && (isManage || o.UserId == User.Oid) && o.Status < EntityStatus.Inactivated)
                .Select(o => new
                {
                    SupplierId = o.SellerId,
                    o.Currency,
                    o.Culture
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (po == null)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.POId));
            }

            var productRQ = new QueryForPurchaseRQ
            {
                SupplierId = po.SupplierId,
                Currency = po.Currency,
                Culture = po.Culture,
                Ids = [rq.ProductId]
            };

            var products = await _productService.QueryForPurchaseAsync(productRQ, true, cancellationToken);
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

            var price = rq.Price;
            var poPrice = _productService.GetPurchasePrice(product);

            var promotion = new PromotionCodeLine
            {
                Price = price,
                Qty = qty
            };

            var amount = price * qty;

            var (linePromotions, lineResult) = _productService.ValidatePromotions(rq.Promotions, product.Promotions, amount, promotion);
            if (!lineResult.Ok)
            {
                return lineResult;
            }

            var title = rq.Title ?? product.Name;
            var discount = linePromotions?.Sum(p => p.Amount) ?? 0;
            var netAmount = amount - discount;

            var orderLine = new OrderLine
            {
                OrderId = poId,
                ProductId = rq.ProductId,
                Title = title,
                Description = rq.Description,
                OriginalPrice = poPrice.GetValueOrDefault(),
                CostPrice = price, // Cost price is the same in purchase order
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

                var result = await _poService.RecalculateAsync(poId, false, cancellationToken);
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

            var id = orderLine.Id;

            // Push message
            var message = new CreatePOLineMessage
            {
                Data = User.CreateMessageData(App.AppId, id, orderLine.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.POLineCreateRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CreatePOLineMessage, cancellationToken);

            return ActionResult.Succeed(id);
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
            var (isEdit, isManage) = await _poService.CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            if (!isEdit)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var poline = await _db.OrderLines
                .Where(q => q.Id == id && q.Order.CoreOrganizationId == orgId
                            && q.Order.Kind == OrderKind.PO
                            && q.Order.Status < EntityStatus.Inactivated
                            && q.Status < EntityStatus.Inactivated
                            && (isManage || q.Order.UserId == User.Oid)
                            && q.AssetId == null)
                .Select(q => new { q.Title, q.OrderId })
                .FirstOrDefaultAsync(cancellationToken);

            if (poline == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(id));
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await _db.OrderLines.Where(q => q.Id == id).ExecuteDeleteAsync(cancellationToken);

                var result = await _poService.RecalculateAsync(poline.OrderId, false, cancellationToken);

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

            // Push message
            var message = new DeletePOLineMessage
            {
                Data = User.CreateMessageData(App.AppId, id, poline.Title)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.DeletePOLineMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// List purchase line JSON data
        /// 采购行列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task ListAsync(POLineListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            return CreateQuery(rq)
                .OrderByDescending(c => c.Creation)
                .ThenBy(c => c.Id)
                .Select(p => new POLineListData
                {
                    Id = p.Id,
                    Title = p.Title,
                    Qty = p.Qty
                })
                .ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query all purchase lines
        /// 查询所有采购行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<POLineQueryAllData[]> QueryAllAsync(POLineQueryAllRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.PO.Query, (short)Permissions.PO.Manage], cancellationToken);
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

                if (rq.SupplierId.HasValue)
                {
                    q = q.Where(p => p.Order.SellerId == rq.SupplierId.Value);
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
            .Select(p => new POLineQueryAllData
            {
                Id = p.Id,
                Source = p.Order.Source,
                Supplier = p.Order.Seller.Name,
                SupplierId = p.Order.SellerId,
                POId = p.OrderId,
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
                Creation = p.Creation,
                BomId = p.BomId
            })
            .ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Query purchase line
        /// 查询采购行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<POLineQueryData[]> QueryAsync(POLineQueryRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.PO.View, (short)Permissions.PO.Manage], cancellationToken);
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
            .Select(p => new POLineQueryData
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                Qty = p.Qty,
                QtyDelivered = p.QtyDelivered,
                Amount = p.Amount,
                Discount = p.Discount,
                StartTime = p.StartTime,
                EndTime = p.EndTime,
                Status = p.Status,
                Creation = p.Creation,
                BomId = p.BomId
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
        public async Task<POLineViewData?> ReadAsync(long id, CancellationToken cancellationToken = default)
        {
            // Permission check
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.PO.View, (short)Permissions.PO.Execute, (short)Permissions.PO.Manage], cancellationToken);
            var isView = permissions[0];
            var isExecute = permissions[1];
            var isManage = permissions[2];
            if (!isView && !isExecute)
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            var data = await _db.OrderLines.AsNoTracking()
                .Where(p => p.Id == id && p.Order.CoreOrganizationId == orgId && p.Order.Kind == OrderKind.PO)
                .Select(p => new POLineViewData
                {
                    Id = p.Id,
                    BuyerId = p.Order.BuyerId,
                    POTitle = p.Order.Title,
                    POId = p.OrderId,
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
                    POUserId = p.Order.UserId,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier == null ? null : p.Supplier.Name,
                    AssetId = p.AssetId,
                    AssetSn = (p.Asset == null ? null : p.Asset.Sn),
                    Modifiers = p.Product.Modifiers,
                    Data = p.Data,
                    Status = p.Status,
                    POStatus = p.Order.Status,
                    Creation = p.Creation,
                    BomId = p.BomId,
                    BomTitle = p.Bom == null ? null : p.Bom.Title
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data != null)
            {
                var userId = User.Oid;
                var now = DateTimeOffset.UtcNow;

                data.IsStartable = isExecute
                    && data.Status < EntityStatus.Inactivated
                    && data.POStatus < EntityStatus.Inactivated
                    && (data.UserId == null || data.StartTime == null);

                data.IsCompletable = isExecute
                        && data.Status < EntityStatus.Inactivated
                        && data.POStatus < EntityStatus.Inactivated
                        && (isManage || data.POUserId == userId || data.UserId == userId)
                        && (data.StartTime < now || (data.AssetQty > 0 && data.AssetId == null));

                data.IsRestorable = isExecute
                        && (isManage || data.POUserId == userId || data.UserId == userId)
                        && data.Status != EntityStatus.Completed && data.Status != EntityStatus.Normal;

                // Push message
                var message = new ReadPOLineMessage
                {
                    Data = User.CreateMessageData(App.AppId, id, data.Title)
                };
                await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.ReadPOLineMessage, cancellationToken);
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
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.PO.Execute, (short)Permissions.PO.Manage], cancellationToken);
            var isExecute = permissions[0];
            var isManage = permissions[1];
            if (!isExecute)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var userId = User.Oid;

            var data = await _db.OrderLines
                .Where(p => p.Id == id
                          && p.Order.CoreOrganizationId == orgId
                          && p.Order.Kind == OrderKind.PO
                          && (isManage || p.Order.UserId == userId || p.UserId == userId)
                          && p.Status != EntityStatus.Normal && p.Status != EntityStatus.Completed
                ).Select(p => new
                {
                    p.Order.BuyerId,
                    p.Title,
                    p.ProductId,
                    p.Qty,
                    p.AssetId,
                    p.AssetQty,
                    p.StartTime,
                    p.EndTime,
                    p.UserId,
                    p.SupplierId,
                    p.Status
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (data.AssetId.HasValue)
                {
                    var assetResult = await _commonService.SyncAssetAsync(data.BuyerId, data.AssetId.Value, -data.AssetQty, data.Qty, cancellationToken);
                    if (!assetResult.Ok)
                    {
                        await transaction.RollbackAsync(cancellationToken);

                        return assetResult;
                    }
                }

                var line = new OrderLine
                {
                    Id = id,
                    StartTime = data.StartTime,
                    EndTime = data.EndTime,
                    UserId = data.UserId,
                    AssetId = data.AssetId,
                    SupplierId = data.SupplierId,
                    Status = data.Status
                };
                _db.OrderLines.Attach(line);

                // Update
                line.StartTime = null;
                line.EndTime = null;
                line.UserId = null;
                line.AssetId = null;
                line.SupplierId = null;
                line.Status = EntityStatus.Normal;

                // Changes
                var changes = _db.ChangeTracker.Entries().GetChangedProperties();

                // Save
                await _db.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                // Push message
                var message = new RollbackPOLineMessage
                {
                    Data = User.CreateMessageData(App.AppId, id, data.Title),
                    Changes = changes
                };
                await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.RollbackPOLineMessage, cancellationToken);
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
        public async Task<IActionResult> StartAsync(POLineStartRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.PO.Execute, (short)Permissions.PO.Manage], cancellationToken);
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
                          && p.Order.Kind == OrderKind.PO
                          && p.Order.Status < EntityStatus.Inactivated
                          && p.Status < EntityStatus.Inactivated)
                .ExecuteUpdateAsync(p => p.SetProperty(ol => ol.StartTime, ol => initStart || ol.StartTime == null ? now : ol.StartTime)
                                        .SetProperty(ol => ol.UserId, ol => (isManage || ol.Order.UserId == userId) ? rq.UserId ?? ol.UserId ?? userId : ol.UserId ?? userId)
                                        .SetProperty(ol => ol.Status, ol => ol.Status == EntityStatus.Normal ? EntityStatus.Doing : ol.Status), cancellationToken);

            if (result > 0)
            {
                var message = new StartPOLineMessage
                {
                    Data = User.CreateMessageData(App.AppId, id),
                    JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.POLineStartRQ)
                };
                await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.StartPOLineMessage, cancellationToken);
            }

            return result > 0 ? ActionResult.Succeed(id) : ApplicationErrors.NoId.AsResult();
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(POLineUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await _poService.CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var orderLine = await _db.OrderLines
                .Where(p => p.Id == rq.Id
                          && p.Order.CoreOrganizationId == orgId
                          && p.Order.Kind == OrderKind.PO
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

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            if (amountUpdated)
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    await _db.SaveChangesAsync(cancellationToken);

                    var result = await _poService.RecalculateAsync(orderLine.OrderId, false, cancellationToken);

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

            // Push message
            var message = new UpdatePOLineMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, orderLine.Title),
                Changes = changes
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdatePOLineMessage, cancellationToken);

            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read data for update
        /// 读取用于更新的数据
        /// </summary>
        /// <param name="id">Order line id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<POLineUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await _poService.CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return null;
            }

            var orgId = User.OrganizationInt;
            var userId = User.Oid;

            return await _db.OrderLines.AsNoTracking()
                .Where(p => p.Id == id && p.Order.CoreOrganizationId == orgId && p.Order.Kind == OrderKind.PO)
                .Select(p => new POLineUpdateReadData
                {
                    Id = p.Id,
                    POId = p.OrderId,
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
