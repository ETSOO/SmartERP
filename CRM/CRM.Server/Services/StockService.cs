using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using CRM.Server.Application;
using CRM.Server.Dto.Stock;
using CRM.Server.RQ;
using CRM.Server.RQ.Stock;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Stock;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using PlatformShared.Services;
using System.Buffers;
using System.Text.Json;

namespace CRM.Server.Services
{
    /// <summary>
    /// Stock service
    /// 库存服务
    /// </summary>
    public class StockService : MyUserService, IStockService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public StockService(
            MyDbContext db,
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<StockService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, userAccessor.UserSafe, "stock", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
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
            var scope = ProductScope.Inventory | ProductScope.Production;
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

            var id = stock.Id;

            // Push message
            var message = new StockAssembleMessage
            {
                Data = User.CreateMessageData(App.AppId, id, stock.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.StockAssembleRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.StockAssembleMessage, cancellationToken);

            // Return result
            return ActionResult.Succeed(id);
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

        private async Task<IActionResult> CheckStockOrderItemsAsync(int orgId, int? locationId, List<long> orders, StockOrderItem[] items, CancellationToken cancellationToken)
        {
            var lineIds = items.Select(i => i.OrderLineId).ToArray();

            var lines = await _db.OrderLines.AsNoTracking()
                .Where(ol => lineIds.Contains(ol.Id)
                             && (ol.QtyDelivered == null || ol.Qty > ol.QtyDelivered)
                             && orders.Contains(ol.OrderId)
                             && (ol.Product.Scope & ProductScope.Inventory) > 0
                             && ol.Order.Status < EntityStatus.Inactivated
                             && ol.Order.CoreOrganizationId == orgId)
                .Select(ol => new StockCheckItemData
                {
                    Id = ol.Id,
                    OrderId = ol.OrderId,
                    Qty = ol.Qty,
                    QtyDelivered = ol.QtyDelivered
                })
                .ToArrayAsync(cancellationToken);

            if (lines.Length != lineIds.Length)
            {
                return ApplicationErrors.NoId.AsResult(nameof(items));
            }
            else
            {
                foreach (var line in lines)
                {
                    var item = items.First(item => item.OrderLineId == line.Id);
                    if (line.QtyDelivered + item.Qty > line.Qty)
                    {
                        return ApplicationErrors.NoValidData.AsResult(nameof(item.Qty));
                    }
                }

                if (locationId.HasValue)
                {
                    // Check stock
                    var stockItems = items.GroupBy(i => i.ProductId).Select(g => new StockItem
                    {
                        ProductId = g.Key,
                        Qty = g.Sum(i => i.Qty)
                    }).ToArray();

                    var checkStockResult = await CheckStockAsync(locationId.Value, stockItems, cancellationToken);
                    if (!checkStockResult.Ok)
                    {
                        return checkStockResult;
                    }
                }
            }

            orders.Clear();
            orders.AddRange(lines.Select(l => l.OrderId).Distinct());

            return ActionResult.Success;
        }

        /// <summary>
        /// Create stock line, only for order & PO
        /// 创建库存行，仅限订单和采购
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateLineAsync(StockCreateLineRQ rq, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var stockId = rq.StockId;
            var validDate = GetDeletableDate();

            var stock = await _db.Stocks(orgId).AsNoTracking()
                                 .Where(s => s.Id == stockId && (s.Kind == StockKind.Order || s.Kind == StockKind.PO) && s.Creation >= validDate)
                                 .Select(s => new StockHeader
                                 {
                                     Id = s.Id,
                                     Kind = s.Kind,
                                     LocationFromId = s.LocationFromId,
                                     LocationToId = s.LocationToId,
                                     OrderIds = s.OrderIds,
                                     TotalLines = s.TotalLines,
                                     TotalQty = s.TotalQty
                                 })
                                 .FirstOrDefaultAsync(cancellationToken);

            if (stock == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.StockId));
            }

            var orderLineId = rq.OrderLineId;

            var orderLine = await _db.OrderLines.AsNoTracking()
                .Where(ol => ol.Id == orderLineId)
                .Select(ol => new { ol.OrderId, ol.ProductId, HasStockLine = ol.StockLines.Any() })
                .FirstOrDefaultAsync(cancellationToken);

            if (orderLine == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.OrderLineId));
            }
            else if (orderLine.HasStockLine)
            {
                return ApplicationErrors.ItemExists.AsResult(nameof(rq.OrderLineId));
            }

            var orderId = orderLine.OrderId;
            var productId = orderLine.ProductId;

            var qty = rq.Qty;

            var isOrder = stock.Kind == StockKind.Order;
            var locationId = isOrder ? stock.LocationFromId : stock.LocationToId;

            var stockOrderItem = new StockOrderItem
            {
                OrderLineId = orderLineId,
                ProductId = productId,
                Qty = qty
            };

            var checkResult = await CheckStockOrderItemsAsync(orgId, isOrder ? locationId : null, [orderId], [stockOrderItem], cancellationToken);
            if (!checkResult.Ok)
            {
                return checkResult;
            }

            _db.StockHeaders.Attach(stock);

            if (stock.OrderIds == null)
            {
                stock.OrderIds = [orderId];
            }
            else if (!stock.OrderIds.Contains(orderId))
            {
                stock.OrderIds.Add(orderId);
            }

            stock.TotalLines += 1;
            stock.TotalQty += qty;

            var lineQty = stock.Kind == StockKind.Order ? -qty : qty;

            var line = new StockLine
            {
                StockId = stockId,
                ProductId = productId,
                Qty = lineQty,
                LocationId = locationId,
                OrderLineId = orderLineId
            };

            _db.StockLines.Add(line);

            await _db.SaveChangesAsync(cancellationToken);

            var id = line.Id;

            // Push message
            var message = new StockCreateLineMessage
            {
                Data = User.CreateMessageData(App.AppId, id, stock.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.StockCreateLineRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.StockCreateLineMessage, cancellationToken);

            return ActionResult.Succeed(id);
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

                    if (rq.ProductId.HasValue)
                    {
                        q = q.Where(s => s.Lines.Any(l => l.ProductId == rq.ProductId.Value));
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

            var stock = await _db.Stocks(orgId).AsNoTracking()
                .Where(s => s.Id == id && s.Creation >= validDate)
                .Select(s => new { s.Title })
                .FirstOrDefaultAsync(cancellationToken);

            if (stock == null)
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

            // Push message
            var message = new DeleteStockMessage
            {
                Data = User.CreateMessageData(App.AppId, id, stock.Title)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.DeleteStockMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Document action data
        /// 文档操作数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<AppActionData?> DocumentActionAsync(DocumentActionRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Document, cancellationToken))
            {
                return null;
            }

            var targetId = rq.TargetId;

            var hasTarget = await _db.Stocks(User.OrganizationInt).AsNoTracking().AnyAsync(p => p.Id == targetId, cancellationToken);
            if (!hasTarget)
            {
                return null;
            }

            var actionName = ServiceConstants.DocumentGenerationAction(rq.Id);

            return App.SignAction(actionName, targetId);
        }

        /// <summary>
        /// Stock loss
        /// 库存报损
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> LoseAsync(StockLoseRQ rq, CancellationToken cancellationToken = default)
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
            var totalQty = -lines.Sum(l => l.Qty);
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

            var id = stock.Id;

            // Push message
            var message = new StockLoseMessage
            {
                Data = User.CreateMessageData(App.AppId, id, stock.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.StockLoseRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.StockLoseMessage, cancellationToken);

            // Return result
            return ActionResult.Succeed(id);
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

            var id = stock.Id;

            // Push message
            var message = new StockInitMessage
            {
                Data = User.CreateMessageData(App.AppId, id, stock.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.StockInitRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.StockInitMessage, cancellationToken);

            // Return result
            return ActionResult.Succeed(id);
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
        public async Task<StockQueryLineData[]> QueryLinesAsync(StockQueryLineRQ rq, CancellationToken cancellationToken)
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
                .Select(l => new StockQueryLineData
                {
                    Id = l.Id,
                    ProductId = l.ProductId,
                    ProductName = l.Product.Name,
                    Qty = l.Qty,
                    OrderLineId = l.OrderLineId
                }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Query order line stock
        /// 查询订单行库存
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<StockQueryOrderLineData[]> QueryOrderLinesAsync(StockQueryOrderLineRQ rq, CancellationToken cancellationToken)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Query, cancellationToken))
            {
                return [];
            }

            var orgId = User.OrganizationInt;
            var personId = rq.PersonId;
            var locationId = rq.LocationId;
            var orders = rq.Orders.ToArray();
            var stockId = rq.StockId;

            // Query all lines
            var query = _db.OrderLines.AsNoTracking()
                .Where(ol => orders.Contains(ol.OrderId)
                    && (ol.QtyDelivered == null || ol.Qty > ol.QtyDelivered)
                    && ol.Order.CoreOrganizationId == orgId
                    && ol.Order.Status < EntityStatus.Inactivated
                    && (ol.Order.SellerId == personId || ol.Order.BuyerId == personId));

            if (stockId.HasValue)
            {
                query = query.Where(ol => !ol.StockLines.Any(sl => sl.StockId == stockId));
            }

            var lines = await query.Select(ol => new
                {
                    ol.Id,
                    ol.ProductId,
                    ol.Qty,
                    PendingQty = ol.Qty - ol.QtyDelivered.GetValueOrDefault(),
                    ol.OrderId
                })
                .ToArrayAsync(cancellationToken);

            var productIds = lines.Select(l => l.ProductId).Distinct().ToArray();
            var defaultScope = ProductScope.Inventory;

            var stocks = await _db.Products(orgId).AsNoTracking()
                .Where(p => productIds.Contains(p.Id) && (p.Scope & defaultScope) > 0)
                .LeftJoin(_db.StockSites.AsNoTracking().Where(s => s.LocationId == locationId), p => p.Id, s => s.ProductId, (p, s) => new
                {
                    p.Id,
                    p.Name,
                    p.AssignedId,
                    p.StepQty,
                    UnitName = p.Unit.Name,
                    p.QueryKeyword,
                    Qty = s == null ? 0 : s.Qty
                })
                .ToArrayAsync(cancellationToken);

            return [.. stocks.Select(s => {
                // Same product lines
                var items = lines.Where(l => l.ProductId == s.Id).OrderBy(l => l.PendingQty);

                var (qty, pendingQty, itemLines) = items.Aggregate(
                    (Qty: 0m, PendingQty: 0m, Lines: new List<StockQueryOrderLineItemData>()),
                    (acc, i) =>
                    {
                        var pendingQty = i.PendingQty;
                        acc.Lines.Add(new StockQueryOrderLineItemData
                        {
                            Id = i.Id,
                            OrderId = i.OrderId,
                            Qty = i.Qty,
                            PendingQty = pendingQty
                        });
                        return (acc.Qty + i.Qty, acc.PendingQty + pendingQty, acc.Lines);
                    }
                );

                return new StockQueryOrderLineData
                {
                    Id = s.Id,
                    Name = s.Name,
                    AssignedId = s.AssignedId,
                    UnitName = s.UnitName,
                    StepQty = s.StepQty,
                    StockQty = s.Qty,
                    OrderQty = qty,
                    PendingQty = pendingQty,
                    Lines = itemLines
                };
            })];
        }

        /// <summary>
        /// Query product lines
        /// 查询产品行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<StockQueryProductLineData[]> QueryProductLinesAsync(StockQueryProductLineRQ rq, CancellationToken cancellationToken)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Query, cancellationToken))
            {
                return [];
            }

            var orgId = User.OrganizationInt;

            return await _db.StockLines.AsNoTracking()
                .Where(sl => sl.ProductId == rq.ProductId && sl.Stock.OrganizationId == orgId)
                .QueryEtsoo(rq, (p) => p.Id, null, (q) =>
                {
                    if (rq.StockKind.HasValue)
                    {
                        q = q.Where(p => p.Stock.Kind == rq.StockKind.Value);
                    }

                    if (rq.LocationId.HasValue)
                    {
                        q = q.Where(p => p.LocationId == rq.LocationId.Value);
                    }

                    if (rq.QtyStart.HasValue)
                    {
                        q = q.Where(p => p.Qty >= rq.QtyStart.Value);
                    }

                    if (rq.QtyEnd.HasValue)
                    {
                        q = q.Where(p => p.Qty <= rq.QtyEnd.Value);
                    }

                    if (rq.CreationStart.HasValue)
                    {
                        q = q.Where(p => p.Stock.Creation >= rq.CreationStart.Value);
                    }

                    if (rq.CreationEnd.HasValue)
                    {
                        q = q.Where(p => p.Stock.Creation <= rq.CreationEnd.Value);
                    }

                    return q;
                })
                .Select(l => new StockQueryProductLineData
                {
                    Id = l.Id,
                    StockId = l.StockId,
                    Title = l.Stock.Title,
                    LocationId = l.LocationId,
                    LocationName = l.Location.Name,
                    Qty = l.Qty,
                    OrderLineId = l.OrderLineId,
                    Creation = l.Stock.Creation
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

            var defaultScope = ProductScope.Inventory;

            rq.Enabled ??= true;

            var locationId = rq.LocationId;

            // Query
            return await _db.Products(orgId).AsNoTracking()
                .Where(p => (p.Scope & defaultScope) > 0)
                .LeftJoin(_db.StockSites.AsNoTracking().Where(s => s.LocationId == locationId), p => p.Id, s => s.ProductId, (p, s) => new
                {
                    p.Id,
                    p.Scope,
                    p.Status,
                    p.Usage,
                    p.CategoryIds,
                    p.CategoryIdsAll,
                    p.Name,
                    p.Description,
                    p.AssignedId,
                    p.StepQty,
                    p.UnitId,
                    UnitName = p.Unit.Name,
                    p.QueryKeyword,
                    Qty = s == null ? (decimal?)null : s.Qty
                })
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

                    if (rq.HasStockQty.HasValue)
                    {
                        if (rq.HasStockQty.Value)
                        {
                            q = q.Where(p => p.Qty > 0);
                        }
                        else
                        {
                            q = q.Where(p => p.Qty == null || p.Qty <= 0);
                        }
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
                }).Select(p => new StockQueryProductData
                {
                    Id = p.Id,
                    Name = p.Name,
                    AssignedId = p.AssignedId,
                    StepQty = p.StepQty,
                    Qty = p.Qty,
                    UnitName = p.UnitName
                }).ToArrayAsync(cancellationToken);
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
                .Where(a => a.Id == locationToId && a.Person.OrgId == orgId)
                .AnyAsync(cancellationToken);

            if (!hasCustomerLocation)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.LocationToId));
            }

            var orders = rq.Orders.ToList();

            // Items check
            var items = rq.Items.ToArray();

            var checkResult = await CheckStockOrderItemsAsync(orgId, locationFromId, orders, items, cancellationToken);
            if (!checkResult.Ok)
            {
                return checkResult;
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
                OrderIds = orders,
                TotalLines = totalLines,
                TotalQty = totalQty,

                Lines = lines
            };

            // Add to database
            _db.StockHeaders.Add(stock);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            var id = stock.Id;

            // Push message
            var message = new StockOrderOutMessage
            {
                Data = User.CreateMessageData(App.AppId, id, stock.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.StockOrderOutRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.StockOrderOutMessage, cancellationToken);

            // Return result
            return ActionResult.Succeed(id);
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
                .Where(a => a.Id == locationFromId && a.Person.OrgId == orgId)
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

            var pos = rq.POs.ToList();

            // Items check
            var items = rq.Items.ToArray();
            var checkResult = await CheckStockOrderItemsAsync(orgId, null, pos, items, cancellationToken);
            if (!checkResult.Ok)
            {
                return checkResult;
            }

            var lines = items.Select(i => new StockLine
            {
                ProductId = i.ProductId,
                Qty = i.Qty,
                LocationId = locationToId,
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
                OrderIds = pos,
                TotalLines = totalLines,
                TotalQty = totalQty,

                Lines = lines
            };

            // Add to database
            _db.StockHeaders.Add(stock);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            var id = stock.Id;

            // Push message
            var message = new StockPOInMessage
            {
                Data = User.CreateMessageData(App.AppId, id, stock.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.StockPOInRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.StockPOInMessage, cancellationToken);

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
                     Orders = _db.OrderAndPOs(orgId).AsNoTracking()
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

            if (data != null)
            {
                data.IsDeletable = data.Creation >= GetDeletableDate();

                // Push message
                var message = new ReadStockMessage
                {
                    Data = User.CreateMessageData(App.AppId, id, data.Title)
                };
                await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.ReadStockMessage, cancellationToken);
            }

            return data;
        }

        /// <summary>
        /// Read stock line
        /// 读取库存行
        /// </summary>
        /// <param name="id">Line id</param>
        /// <param name="checkPermission">Check permission or not</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<StockLineViewData?> ReadLineAsync(long id, bool checkPermission, CancellationToken cancellationToken = default)
        {
            if (checkPermission && !await _commonService.HasPermissionAsync((short)Permissions.Inventory.View, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.StockLines.AsNoTracking()
                .Where(sl => sl.Id == id && sl.Stock.OrganizationId == orgId)
                .LeftJoin(_db.StockSites.AsNoTracking(), sl => new { sl.ProductId, LocationId = (int?)sl.LocationId }, s => new { s.ProductId, s.LocationId }, (sl, s) => new StockLineViewData
                {
                    Id = sl.Id,
                    StockId = sl.StockId,
                    StockKind = sl.Stock.Kind,
                    ProductId = sl.ProductId,
                    StepQty = sl.Product.StepQty,
                    OrderLineId = sl.OrderLineId,
                    Qty = sl.Qty,
                    OrderQty = sl.OrderLine == null ? null : sl.OrderLine.Qty,
                    PendingQty = sl.OrderLine == null ? null : sl.OrderLine.Qty - sl.OrderLine.QtyDelivered.GetValueOrDefault(),
                    StockQty = s == null ? 0 : s.Qty
                })
                .FirstOrDefaultAsync(cancellationToken);
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

            var stock = await _db.Stocks(orgId).AsNoTracking()
                .Where(s => s.Id == id && s.ReceiptTime == null)
                .Select(s => new { s.Title, s.LocationFromId, s.LocationToId })
                .FirstOrDefaultAsync(cancellationToken);

            if (stock == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var now = DateTimeOffset.UtcNow;

            var locationFromId = stock.LocationFromId;
            var locationToId = stock.LocationToId;

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await _db.Database.ExecuteSqlAsync($@"
                    INSERT INTO stock_line
                    (
                        stock_id,
                        product_id,
                        location_id,
                        qty,
                        order_line_id
                    )
                    SELECT
                        {id},
                        product_id,
                        CASE
                            WHEN location_id = {locationToId} THEN {locationFromId}
                            ELSE {locationToId}
                        END,
                        -qty,
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

            // Push message
            var message = new StockReceiveMessage
            {
                Data = User.CreateMessageData(App.AppId, id, stock.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.StockReceiveRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.StockReceiveMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Report action data
        /// 报表操作数据
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<AppActionData?> ReportActionAsync(CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Report, cancellationToken))
            {
                return null;
            }

            return App.SignAction(ServiceConstants.ReportInventoryAction, User.Pid);
        }

        /// <summary>
        /// Stock take
        /// 库存盘点
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> TakeAsync(StockTakeRQ rq,  CancellationToken cancellationToken = default)
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

            // Stock check for loss, not for gain
            var lossItems = items.Where(i => i.Qty < 0).Select(i => new StockItem
            {
                ProductId = i.ProductId,
                Qty = -i.Qty
            }).ToArray();
            var checkStockResult = await CheckStockAsync(locationId, lossItems, cancellationToken);
            if (!checkStockResult.Ok)
            {
                return checkStockResult;
            }

            var lines = items.Select(i => new StockLine
            {
                ProductId = i.ProductId,
                Qty = i.Qty,
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

            var id = stock.Id;

            // Push message
            var message = new StockTakeMessage
            {
                Data = User.CreateMessageData(App.AppId, id, stock.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.StockTakeRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.StockTakeMessage, cancellationToken);

            // Return result
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
            var totalQty = -lines.Sum(l => l.Qty);
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
                TrackingNumber = rq.TrackingNumber?.Trim().ToUpper(),
                Creation = now,
                TotalLines = totalLines,
                TotalQty = totalQty,

                Lines = lines
            };

            // Add to database
            _db.StockHeaders.Add(stock);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            var id = stock.Id;

            // Push message
            var message = new StockTransferMessage
            {
                Data = User.CreateMessageData(App.AppId, id, stock.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.StockTransferRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.StockTransferMessage, cancellationToken);

            // Return result
            return ActionResult.Succeed(id);
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

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateStockMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, stock.Title),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateStockMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Update stock line
        /// 更新库存行
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateLineAsync(StockUpdateLineRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Inventory.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;
            var id = rq.Id;

            var stockLine = await ReadLineAsync(id, false, cancellationToken);
            if (stockLine == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var stockId = stockLine.StockId;
            var kind = stockLine.StockKind;

            if (kind != StockKind.Order && kind != StockKind.PO)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(stockLine.StockKind));
            }

            // For order, it's negative
            var currentQty = stockLine.Qty;
            var pendingQty = stockLine.PendingQty;

            // New qty is always positive or zero
            var qty = rq.Qty;

            var adjustQty = qty - Math.Abs(currentQty);

            if (adjustQty == 0 || adjustQty > pendingQty)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.Qty));
            }
            else if (kind == StockKind.Order && adjustQty > stockLine.StockQty)
            {
                return LocalAppErrors.InsufficientStock.AsResult();
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (qty == 0)
                {
                    await _db.StockLines.AsNoTracking().Where(l => l.Id == id).ExecuteDeleteAsync(cancellationToken);
                }
                else
                {
                    var newQty = kind == StockKind.Order ? -qty : qty;
                    await _db.StockLines.AsNoTracking().Where(l => l.Id == id).ExecuteUpdateAsync(l => l.SetProperty(p => p.Qty, newQty), cancellationToken);
                }

                var orderIds = await _db.StockLines
                    .Where(x => x.StockId == stockId && x.OrderLineId != null)
                    .Join(
                        _db.OrderLines,
                        stockLine => stockLine.OrderLineId,
                        orderLine => orderLine.Id,
                        (stockLine, orderLine) => orderLine.OrderId
                    )
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var stock = await _db.Stocks(orgId).AsNoTracking()
                    .Where(s => s.Id == stockId)
                    .Select(s => new StockHeader { Id = s.Id, TotalLines = s.TotalLines, TotalQty = s.TotalQty, OrderIds = s.OrderIds })
                    .FirstAsync(cancellationToken);

                _db.StockHeaders.Attach(stock);

                stock.TotalQty += adjustQty;
                stock.OrderIds = orderIds;

                if (qty == 0)
                {
                    stock.TotalLines -= 1;
                }

                await _db.SaveChangesAsync(cancellationToken);

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

            // Push message
            var message = new UpdateStockLineMessage
            {
                Data = User.CreateMessageData(App.AppId, id),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.StockUpdateLineRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateStockLineMessage, cancellationToken);

            // Return
            return ActionResult.Succeed(id);
        }
    }
}
