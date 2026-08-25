using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.String;
using CRM.Server.Application;
using CRM.Server.Dto;
using CRM.Server.Dto.PO;
using CRM.Server.RQ.PO;
using CRM.Server.RQ.Product;
using CRM.Server.RQ.Supplier;
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
    /// Purchase order service
    /// 采购订单服务
    /// </summary>
    public class POService : MyUserService, IPOService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly ISupplierService _supplierService;
        readonly IProductService _productService;
        readonly IQueueService _queueService;

        public POService(
            MyDbContext db,
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<POService> logger,
            ICommonService commonService,
            ISupplierService supplierService,
            IProductService productService,
            IQueueService queueService
        )
            : base(app, userAccessor.UserSafe, "po", logger)
        {
            _db = db;
            _commonService = commonService;
            _supplierService = supplierService;
            _productService = productService;
            _queueService = queueService;
        }

        IActionResult CreateNoValidDataResult(string field, decimal targetValue, decimal currentValue, string product)
        {
            var result = ApplicationErrors.NoValidData.AsResult(field);
            result.Detail = $"{targetValue}|{currentValue}|{product}";
            return result;
        }

        /// <summary>
        /// Check edit permissions
        /// 检查编辑权限
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<(bool IsEdit, bool IsManage)> CheckEditPermissionsAsync(CancellationToken cancellationToken = default)
        {
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.PO.Edit, (short)Permissions.PO.Manage], cancellationToken);
            return (permissions[0], permissions[1]);
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(POCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.PO.Add, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Check supplier data
            var supplierRQ = new SupplierReadForPurchaseRQ
            {
                SupplierId = rq.SupplierId,
                Currency = rq.Currency
            };
            var supplier = await _supplierService.ReadForPurchaseAsync(supplierRQ, cancellationToken);
            if (supplier == null || supplier.Supplier == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.SupplierId));
            }

            var orgId = User.OrganizationInt;
            var supplierId = supplier.Supplier.Id;

            // Check frequency, 30 seconds
            var now = DateTime.UtcNow.AddSeconds(-30);
            var hasOrder = await _db.POs(orgId).Where(o => o.Creation > now).AnyAsync(cancellationToken);
            if (hasOrder)
            {
                return ApplicationErrors.RateLimiting.AsResult();
            }

            // Query all products
            var lineProductIds = rq.Lines.Select(l => l.ProductId).Distinct();
            var productRQ = new QueryForPurchaseRQ
            {
                SupplierId = rq.SupplierId,
                Currency = rq.Currency,
                Culture = rq.Culture,
                Ids = lineProductIds
            };

            var products = await _productService.QueryForPurchaseAsync(productRQ, true, cancellationToken);
            if (products == null || lineProductIds.Count() != products.Length)
            {
                return ApplicationErrors.DataOutdated.AsResult();
            }

            // User
            if (rq.UserId.HasValue)
            {
                var userExists = await _db.Users(orgId).Where(u => u.Id == rq.UserId.Value).AnyAsync(cancellationToken);
                if (!userExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.UserId));
                }
            }

            // Delivery & Payment validation
            if (rq.DeliveryId.HasValue)
            {
                var deliveryExists = await _db.OrderDeliveries(orgId).Where(d => d.Id == rq.DeliveryId.Value && !d.IsOrder).AnyAsync(cancellationToken);
                if (!deliveryExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.DeliveryId));
                }
            }

            if (rq.PaymentId.HasValue)
            {
                var paymentExists = await _db.OrderPayments(orgId).Where(p => p.Id == rq.PaymentId.Value && !p.IsOrder).AnyAsync(cancellationToken);
                if (!paymentExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.PaymentId));
                }
            }

            // Address
            string? addressFormatted = null;
            if (rq.AddressId.HasValue)
            {
                var orgPersonId = User.Pid;
                addressFormatted = await _db.PersonAddresses(orgPersonId).Where(a => a.Id == rq.AddressId.Value).Select(a => a.FormattedAddress).FirstOrDefaultAsync(cancellationToken);
                if (addressFormatted == null)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.AddressId));
                }
            }

            // Contact
            if (rq.ContactId.HasValue)
            {
                var contactExists = await _db.PersonRelations(orgId, supplierId).Where(c => c.ContactId == rq.ContactId.Value).AnyAsync(cancellationToken);
                if (!contactExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.ContactId));
                }
            }

            var title = rq.Title ?? StringUtils.FormatName(supplier.Supplier.Name, 6, 2) + " " + now.ToString("yyyy/MM/dd");

            var lines = new List<OrderLine>();

            var promotionSummary = new PromotionSummary();

            short lineCount = 0;
            decimal items = 0;
            decimal lineDiscount = 0;
            decimal totalAmount = 0;

            foreach (var l in rq.Lines)
            {
                var product = products.FirstOrDefault(p => p.Id == l.ProductId);
                if (product == null)
                {
                    return ApplicationErrors.DataOutdated.AsResult(l.ProductId.ToString());
                }

                var qty = l.Qty;

                var qtyResult = _productService.ValidateQty(product, qty);
                if (qtyResult != null)
                {
                    return qtyResult;
                }

                var purchasePrice = _productService.GetPurchasePrice(product);

                if (!purchasePrice.HasValue && !l.Price.HasValue)
                {
                    return CreateNoValidDataResult(nameof(l.Price), purchasePrice.GetValueOrDefault(), l.Price.GetValueOrDefault(), product.Name);
                }

                var price = l.Price ?? purchasePrice ?? 0;

                var amount = price * qty;

                var sale = new PromotionCodeLine
                {
                    Price = price,
                    Qty = qty
                };

                var (linePromotions, lineResult) = _productService.ValidatePromotions(l.Promotions, product.Promotions, amount, sale);
                if (!lineResult.Ok)
                {
                    return lineResult;
                }

                promotionSummary.Add(linePromotions);

                var lineTitle = l.Title ?? product.Name;
                var discount = l.Promotions?.Sum(p => p.Amount) ?? 0;
                var netAmount = amount - discount;

                lines.Add(new OrderLine
                {
                    ProductId = l.ProductId,
                    Title = lineTitle,
                    Description = l.Description,
                    OriginalPrice = purchasePrice.GetValueOrDefault(),
                    CostPrice = price, // Cost price is the same in purchase order
                    Price = price,
                    Qty = qty,
                    AssetQty = product.AssetQty.GetValueOrDefault(),
                    Amount = netAmount,
                    Discount = discount,
                    Promotions = linePromotions,
                    StartTime = l.StartTime,
                    EndTime = l.EndTime,
                    Data = l.Data,
                    Status = l.Status.GetValueOrDefault()
                });

                lineCount++;
                items += qty;
                lineDiscount += discount;
                totalAmount += netAmount;
            }

            // Validate purchase promotions
            var (purchasePromotions, opResult) = _productService.ValidatePromotions(rq.Promotions, supplier.Supplier.Promotions, totalAmount);
            if (!opResult.Ok)
            {
                return opResult;
            }

            promotionSummary.Add(purchasePromotions);

            // Purchase discount
            var purchaseDiscount = purchasePromotions?.Sum(p => p.Amount) ?? 0;

            // Purchase amount without discount
            var poAmount = totalAmount - purchaseDiscount;

            if (rq.Amount.HasValue && rq.Amount.Value != poAmount)
            {
                return CreateNoValidDataResult(nameof(rq.Amount), poAmount, rq.Amount.Value, "po");
            }

            var userId = rq.UserId ?? User.Oid;

            var po = new OrderHeader
            {
                CoreOrganizationId = orgId,
                UserId = userId,
                Kind = OrderKind.PO,
                SellerId = supplierId,
                BuyerId = User.Pid, // Organization is the buyer
                Source = rq.Source?.ToUpper(),
                SourceId = rq.SourceId?.ToUpper(),
                Title = title,
                Description = rq.Description,
                StartDate = rq.StartDate,
                EndDate = rq.EndDate,
                AssignedId = rq.AssignedId?.ToUpper(),
                Currency = rq.Currency,
                Culture = rq.Culture,
                PaymentId = rq.PaymentId,
                PaymentInstruction = rq.PaymentInstruction,
                DeliveryId = rq.DeliveryId,
                DeliveryInstruction = rq.DeliveryInstruction,
                AddressId = rq.AddressId,
                AddressFormatted = addressFormatted,
                ContactId = rq.ContactId,
                Promotions = purchasePromotions,
                Amount = poAmount,
                Discount = purchaseDiscount,
                LineDiscount = lineDiscount,
                Lines = lineCount,
                Items = items,
                TaxAmount = rq.TaxAmount.GetValueOrDefault(),
                Data = rq.Data,
                Status = rq.Status.GetValueOrDefault(),

                OrderLines = lines
            };

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (rq.Tags?.Any() is true)
                {
                    var tagIds = await _commonService.AddTagsAsync(FeatureTagKind.PO, rq.Tags, cancellationToken);
                    po.Tags = [.. tagIds];
                }

                _db.OrderHeaders.Add(po);

                // Save changes
                await _db.SaveChangesAsync(cancellationToken);

                // Update promotion summary
                await promotionSummary.UpdateAsync(_db, cancellationToken);

                // Commit
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Rollback
                await transaction.RollbackAsync(cancellationToken);

                // Log and return the result
                return LogException(ex);
            }

            var id = po.Id;

            // Push message
            var message = new CreatePOMessage
            {
                Data = User.CreateMessageData(App.AppId, id, po.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.POCreateRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CreatePOMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        private IQueryable<OrderHeader> CreateQuery(POListRQ rq, Func<IQueryable<OrderHeader>, IQueryable<OrderHeader>>? filters = null)
        {
            var query = _db.POs(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (o) => o.Id, (o) => o.Status, (q) =>
                {
                    if (rq.TagId != null)
                    {
                        q = q.Where(p => p.Tags != null && p.Tags.Contains(rq.TagId.Value));
                    }

                    if (!string.IsNullOrEmpty(rq.Source))
                    {
                        q = q.Where(o => o.Source == rq.Source.ToUpper());
                    }

                    if (rq.SupplierId.HasValue)
                    {
                        q = q.Where(o => o.SellerId == rq.SupplierId.Value);
                    }

                    if (!string.IsNullOrEmpty(rq.Currency))
                    {
                        q = q.Where(o => o.Currency == rq.Currency);
                    }

                    if (!string.IsNullOrEmpty(rq.Culture))
                    {
                        q = q.Where(o => o.Culture == rq.Culture);
                    }

                    if (rq.DeliveryId.HasValue)
                    {
                        q = q.Where(o => o.DeliveryId == rq.DeliveryId.Value);
                    }

                    if (rq.PaymentId.HasValue)
                    {
                        q = q.Where(o => o.PaymentId == rq.PaymentId.Value);
                    }

                    if (rq.ProductId.HasValue)
                    {
                        q = q.Where(o => o.OrderLines.Any(l => l.ProductId == rq.ProductId.Value));
                    }

                    if (rq.UserId.HasValue)
                    {
                        q = q.Where(o => o.UserId == rq.UserId.Value);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        if (keyword.IsComplexQueryKeywords())
                        {
                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, a => a.Title, a => a.Description);
                        }
                        else
                        {
                            q = q.Where(ou => EF.Functions.ILike(ou.Title, $"%{keyword}%")
                            || (ou.Description != null && EF.Functions.ILike(ou.Description, $"%{keyword}%"))
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
        /// List order JSON data
        /// 订单列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(POListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _commonService.UpdateTagAsync(rq, User.OrganizationInt, cancellationToken);

            await CreateQuery(rq)
                .Select(o => new POListData
                {
                    Id = o.Id,
                    Title = o.Title
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query order JSON data
        /// 查询订单JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<POQueryData[]> QueryAsync(POQueryRQ rq, CancellationToken cancellationToken = default)
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

            await _commonService.UpdateTagAsync(rq, User.OrganizationInt, cancellationToken);

            return await CreateQuery(rq, (q) =>
            {
                if (!string.IsNullOrEmpty(rq.SourceId))
                {
                    q = q.Where(o => o.SourceId == rq.SourceId.ToUpper());
                }

                if (!string.IsNullOrEmpty(rq.AssignedId))
                {
                    q = q.Where(o => o.AssignedId == rq.AssignedId.ToUpper());
                }

                if (rq.HasPromotion.HasValue)
                {
                    if (rq.HasPromotion.Value)
                    {
                        q = q.Where(o => o.Discount > 0 || o.LineDiscount > 0);
                    }
                    else
                    {
                        q = q.Where(o => o.Discount == 0 && o.LineDiscount == 0);
                    }
                }

                if (rq.CreationStart.HasValue)
                {
                    q = q.Where(o => o.Creation >= rq.CreationStart.Value);
                }

                if (rq.CreationEnd.HasValue)
                {
                    q = q.Where(o => o.Creation < rq.CreationEnd.Value);
                }

                if (rq.StartDateStart.HasValue)
                {
                    q = q.Where(o => o.StartDate.HasValue && o.StartDate.Value >= rq.StartDateStart.Value);
                }

                if (rq.StartDateEnd.HasValue)
                {
                    q = q.Where(o => o.StartDate.HasValue && o.StartDate.Value < rq.StartDateEnd.Value);
                }

                if (rq.AmountStart.HasValue)
                {
                    q = q.Where(o => o.Amount >= rq.AmountStart.Value);
                }

                if (rq.AmountEnd.HasValue)
                {
                    q = q.Where(o => o.Amount < rq.AmountEnd.Value);
                }

                return q;
            })
            .Select(o => new POQueryData
            {
                Id = o.Id,
                Source = o.Source,
                Title = o.Title,
                SupplierId = o.SellerId,
                SupplierName = o.Seller.Name,
                Lines = o.Lines,
                Items = o.Items,
                Currency = o.Currency,
                Amount = o.Amount,
                Discount = o.Discount,
                LineDiscount = o.LineDiscount,
                ApprovedDiscount = o.ApprovedDiscount,
                TaxAmount = o.TaxAmount,
                Status = o.Status,
                StartDate = o.StartDate,
                Creation = o.Creation
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Read data for view
        /// 读取用于浏览的数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<POViewData?> ReadAsync(long id, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.PO.View, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            var po = await _db.POs(orgId).AsNoTracking()
                 .Where(p => p.Id == id)
                 .Select(p => new POViewData
                 {
                     Id = p.Id,
                     Source = p.Source,
                     SourceId = p.SourceId,
                     AssignedId = p.AssignedId,
                     SupplierId = p.SellerId,
                     SupplierName = p.Seller.Name,
                     Title = p.Title,
                     Description = p.Description,
                     StartDate = p.StartDate,
                     EndDate = p.EndDate,
                     Currency = p.Currency,
                     Amount = p.Amount,
                     PaidAmount = p.PaidAmount,
                     Discount = p.Discount,
                     LineDiscount = p.LineDiscount,
                     ApprovedDiscount = p.ApprovedDiscount,
                     TaxAmount = p.TaxAmount,
                     Lines = p.Lines,
                     Items = p.Items,
                     Promotions = p.Promotions,
                     Culture = p.Culture,
                     Payment = p.Payment == null ? null : p.Payment.Title,
                     PaymentInstruction = p.PaymentInstruction,
                     Delivery = p.Delivery == null ? null : p.Delivery.Title,
                     DeliveryInstruction = p.DeliveryInstruction,
                     AddressFormatted = p.AddressFormatted,
                     Contact = p.Contact == null ? null : p.Contact.Name,
                     ContactId = p.ContactId,
                     User = p.User.Name,
                     UserId = p.UserId,
                     Creation = p.Creation,
                     Status = p.Status,
                     Tags = p.Tags == null ? null : _db.FeatureTags.Where(k => k.CoreOrganizationId == orgId && p.Tags.Contains(k.Id)).OrderByDescending(t => t.Total).ThenBy(t => t.Tag).Select(k => k.Tag).ToList(),
                 }).FirstOrDefaultAsync(cancellationToken);

            if (po != null)
            {
                // Push message
                var message = new ReadPOMessage
                {
                    Data = User.CreateMessageData(App.AppId, id, po.Title)
                };
                await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.ReadPOMessage, cancellationToken);
            }

            return po;
        }

        /// <summary>
        /// Recalcuate order amount and promotion
        /// 重新计算订单金额和促销活动
        /// </summary>
        /// <param name="id">Order id</param>
        /// <param name="checkPermission">Check permission or not</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> RecalculateAsync(long id, bool checkPermission, CancellationToken cancellationToken = default)
        {
            bool isLocalManage;
            if (checkPermission)
            {
                var (isEdit, isManage) = await CheckEditPermissionsAsync(cancellationToken);
                if (!isEdit)
                {
                    return ApplicationErrors.AccessDenied.AsResult();
                }

                isLocalManage = isManage;
            }
            else
            {
                isLocalManage = true;
            }

            var orgId = User.OrganizationInt;

            // Fetch order with calculated aggregates in a single query
            var result = await _db.POs(orgId)
                .Where(p => p.Id == id && (isLocalManage || p.UserId == User.Oid) && p.Status < EntityStatus.Inactivated)
                .Select(o => new
                {
                    PO = o,
                    Lines = o.OrderLines.Where(l => l.BomId == null)
                })
                .Select(o => new
                {
                    o.PO,
                    Lines = (short)o.Lines.Count(),
                    Items = o.Lines.Sum(l => l.Qty),
                    LineDiscount = o.Lines.Sum(l => l.Discount),
                    TotalAmount = o.Lines.Sum(l => l.Amount)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var po = result.PO;
            var lines = result.Lines;
            var items = result.Items;
            var lineDiscount = result.LineDiscount;
            var totalAmount = result.TotalAmount;

            // Attach the order to the context for tracking
            _db.Attach(po);

            // Check supplier data
            var supplierRQ = new SupplierReadForPurchaseRQ
            {
                SupplierId = po.SellerId,
                Currency = po.Currency
            };

            var supplier = await _supplierService.ReadForPurchaseAsync(supplierRQ, cancellationToken);
            if (supplier == null || supplier.Supplier == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(supplierRQ.SupplierId));
            }

            // Calculate order promotions
            var promotions = _productService.CalculatePromotions(supplier.Supplier.Promotions, totalAmount);

            // Order discount
            var orderDiscount = promotions?.Sum(p => p.Amount) ?? 0;

            // Order amount without discount
            var orderAmount = totalAmount - orderDiscount;

            // Update
            po.Lines = lines;
            po.LineDiscount = lineDiscount;
            po.Amount = orderAmount;
            po.Discount = orderDiscount;
            po.Promotions = promotions;

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new RecalculatePOMessage
            {
                Data = User.CreateMessageData(App.AppId, id, po.Title),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.RecalculatePOMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            // Return
            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(POUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var order = await _db.POs(orgId)
                .Where(p => p.Id == rq.Id && (isManage || p.UserId == User.Oid))
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            if ((order.Discount > 0 || order.LineDiscount > 0) && rq.IsModified(nameof(rq.CustomerId)))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.CustomerId));
            }

            if (rq.IsModified(nameof(rq.Source)))
            {
                order.Source = rq.Source?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.SourceId)))
            {
                order.SourceId = rq.SourceId?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.CustomerId)) && rq.CustomerId.HasValue)
            {
                var hasCustomer = await _db.Customers(orgId).Where(c => c.Id == rq.CustomerId.Value).AnyAsync(cancellationToken);
                if (!hasCustomer)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.CustomerId));
                }

                order.BuyerId = rq.CustomerId.Value;
            }

            if (rq.IsModified(nameof(rq.Culture)) && !string.IsNullOrEmpty(rq.Culture))
            {
                order.Culture = rq.Culture;
            }

            if (rq.IsModified(nameof(rq.Title)) && !string.IsNullOrEmpty(rq.Title))
            {
                order.Title = rq.Title;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                order.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.PaymentId)))
            {
                if (rq.PaymentId.HasValue)
                {
                    var paymentExists = await _db.OrderPayments(orgId).Where(p => p.Id == rq.PaymentId.Value && !p.IsOrder).AnyAsync(cancellationToken);
                    if (!paymentExists)
                    {
                        return ApplicationErrors.NoId.AsResult(nameof(rq.PaymentId));
                    }
                }

                order.PaymentId = rq.PaymentId;
            }

            if (rq.IsModified(nameof(rq.PaymentInstruction)))
            {
                order.PaymentInstruction = rq.PaymentInstruction;
            }

            if (rq.IsModified(nameof(rq.DeliveryId)))
            {
                if (rq.DeliveryId.HasValue)
                {
                    var deliveryExists = await _db.OrderDeliveries(orgId).Where(d => d.Id == rq.DeliveryId.Value && !d.IsOrder).AnyAsync(cancellationToken);
                    if (!deliveryExists)
                    {
                        return ApplicationErrors.NoId.AsResult(nameof(rq.DeliveryId));
                    }
                }

                order.DeliveryId = rq.DeliveryId;
            }

            if (rq.IsModified(nameof(rq.DeliveryInstruction)))
            {
                order.DeliveryInstruction = rq.DeliveryInstruction;
            }

            if (rq.IsModified(nameof(rq.StartDate)))
            {
                order.StartDate = rq.StartDate;
            }

            if (rq.IsModified(nameof(rq.EndDate)))
            {
                order.EndDate = rq.EndDate;
            }

            if (rq.IsModified(nameof(rq.AddressId)))
            {
                if (rq.AddressId.HasValue)
                {
                    var orgPersonId = User.Pid;
                    var addressFormatted = await _db.PersonAddresses(orgPersonId).Where(a => a.Id == rq.AddressId.Value).Select(a => a.FormattedAddress).FirstOrDefaultAsync(cancellationToken);
                    if (addressFormatted == null)
                    {
                        return ApplicationErrors.NoId.AsResult(nameof(rq.AddressId));
                    }

                    order.AddressFormatted = addressFormatted;
                }
                else
                {
                    order.AddressFormatted = null;
                }

                order.AddressId = rq.AddressId;
            }

            if (rq.IsModified(nameof(rq.ContactId)))
            {
                if (rq.ContactId.HasValue)
                {
                    var contactExists = await _db.PersonRelations(orgId, order.BuyerId).Where(c => c.ContactId == rq.ContactId.Value).AnyAsync(cancellationToken);
                    if (!contactExists)
                    {
                        return ApplicationErrors.NoId.AsResult(nameof(rq.ContactId));
                    }
                }

                order.ContactId = rq.ContactId;
            }

            if (rq.IsModified(nameof(rq.AssignedId)))
            {
                order.AssignedId = rq.AssignedId?.ToUpper();
            }

            if (rq.IsModified(nameof(rq.TaxAmount)) && rq.TaxAmount.HasValue)
            {
                order.TaxAmount = rq.TaxAmount.Value;
            }

            if (rq.IsModified(nameof(rq.Data)))
            {
                order.Data = rq.Data;
            }

            if (rq.IsModified(nameof(rq.UserId)) && rq.UserId.HasValue)
            {
                var userExists = await _db.Users(orgId).Where(u => u.Id == rq.UserId.Value).AnyAsync(cancellationToken);
                if (!userExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.UserId));
                }

                order.UserId = rq.UserId.Value;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                order.Status = rq.Status.Value;
            }

            if (rq.IsModified(nameof(rq.Tags)))
            {
                if (rq.Tags?.Any() is true)
                {
                    var tagIds = await _commonService.AddTagsAsync(FeatureTagKind.PO, rq.Tags, cancellationToken);
                    order.Tags = [.. tagIds];
                }
                else
                {
                    order.Tags = null;
                }
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdatePOMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, order.Title),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdatePOMessage, cancellationToken);

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
        public async Task<POUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.POs(orgId).AsNoTracking()
                .Where(p => p.Id == id && (isManage || p.UserId == User.Oid))
                .Select(p => new POUpdateReadData
                {
                    Id = p.Id,
                    Source = p.Source,
                    SourceId = p.SourceId,
                    SupplierId = p.SellerId,
                    Currency = p.Currency,
                    Culture = p.Culture,
                    Amount = p.Amount,
                    Discount = p.Discount,
                    LineDiscount = p.LineDiscount,
                    Lines = p.Lines,
                    Items = p.Items,
                    Title = p.Title,
                    Description = p.Description,
                    PaymentId = p.PaymentId,
                    PaymentInstruction = p.PaymentInstruction,
                    DeliveryId = p.DeliveryId,
                    DeliveryInstruction = p.DeliveryInstruction,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    AddressId = p.AddressId,
                    ContactId = p.ContactId,
                    AssignedId = p.AssignedId,
                    TaxAmount = p.TaxAmount,
                    UserId = p.UserId,
                    Status = p.Status,
                    Data = p.Data,
                    Tags = p.Tags == null ? null : _db.FeatureTags.Where(k => k.CoreOrganizationId == orgId && p.Tags.Contains(k.Id)).OrderByDescending(t => t.Total).ThenBy(t => t.Tag).Select(k => k.Tag).ToList()
                }).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
