using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.String;
using CRM.Server.Dto;
using CRM.Server.Dto.Order;
using CRM.Server.RQ.Customer;
using CRM.Server.RQ.Order;
using CRM.Server.RQ.Product;
using CRM.Server.Utils;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.Order;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using System.Buffers;
using System.Text.Json;

namespace CRM.Server.Services
{
    /// <summary>
    /// Order service
    /// 订单服务
    /// </summary>
    public class OrderService : SEUserService, IOrderService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly ICustomerService _customerService;
        readonly IProductService _productService;
        readonly IQueueService _queueService;

        public OrderService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<OrderService> logger,
            ICommonService commonService,
            ICustomerService customerService,
            IProductService productService,
            IQueueService queueService
        )
            : base(app, userAccessor.UserSafe, "order", logger)
        {
            _db = db;
            _commonService = commonService;
            _customerService = customerService;
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
            var permissions = await _commonService.HasPermissionsAsync([(short)Permissions.Order.Edit, (short)Permissions.Order.Manage], cancellationToken);
            return (permissions[0], permissions[1]);
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(OrderCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.Add, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Check customer data
            var customerRQ = new CustomerReadForSaleRQ
            {
                CustomerId = rq.CustomerId,
                Currency = rq.Currency
            };
            var customer = await _customerService.ReadForSaleAsync(customerRQ, cancellationToken);
            if (customer == null || customer.Customer == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.CustomerId));
            }

            var orgId = User.OrganizationInt;
            var customerId = customer.Customer.Id;

            // Check frequency, 30 seconds
            var now = DateTime.UtcNow.AddSeconds(-30);
            var hasOrder = await _db.Orders(orgId).Where(o => o.Creation > now).AnyAsync(cancellationToken);
            if (hasOrder)
            {
                return ApplicationErrors.RateLimiting.AsResult();
            }

            // Query all products
            var lineProductIds = rq.Lines.Select(l => l.ProductId).Distinct();
            var productRQ = new QueryForSaleRQ
            {
                CustomerId = rq.CustomerId,
                Currency = rq.Currency,
                Culture = rq.Culture,
                Ids = lineProductIds
            };

            var products = await _productService.QueryForSaleAsync(productRQ, true, cancellationToken);
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
                var deliveryExists = await _db.OrderDeliveries(orgId).Where(d => d.Id == rq.DeliveryId.Value && d.IsOrder).AnyAsync(cancellationToken);
                if (!deliveryExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.DeliveryId));
                }
            }

            if (rq.PaymentId.HasValue)
            {
                var paymentExists = await _db.OrderPayments(orgId).Where(p => p.Id == rq.PaymentId.Value && p.IsOrder).AnyAsync(cancellationToken);
                if (!paymentExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.PaymentId));
                }
            }

            // Address
            string? addressFormatted = null;
            if (rq.AddressId.HasValue)
            {
                addressFormatted = await _db.PersonAddresses(customerId).Where(a => a.Id == rq.AddressId.Value).Select(a => a.FormattedAddress).FirstOrDefaultAsync(cancellationToken);
                if (addressFormatted == null)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.AddressId));
                }
            }

            // Contact
            if (rq.ContactId.HasValue)
            {
                var contactExists = await _db.PersonRelations(orgId, customerId).Where(c => c.ContactId == rq.ContactId.Value).AnyAsync(cancellationToken);
                if (!contactExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.ContactId));
                }
            }

            var title = rq.Title ?? StringUtils.FormatName(customer.Customer.Name, 6, 2) + " " + now.ToString("yyyy/MM/dd");

            var lines = new List<OrderLine>();

            var promotionSummary = new PromotionSummary();

            short lineCount = 0;
            decimal items = 0;
            decimal lineDiscount = 0;
            decimal totalAmount = 0;

            var boms = new OrderBoms();

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

                var price = _productService.GetSalePrice(product);

                if (l.Price.HasValue && l.Price.Value != price)
                {
                    return CreateNoValidDataResult(nameof(l.Price), price, l.Price.Value, product.Name);
                }
                
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

                var line = new OrderLine
                {
                    ProductId = l.ProductId,
                    Title = lineTitle,
                    Description = l.Description,
                    OriginalPrice = product.RetailPrice,
                    CostPrice = product.CostPrice ?? 0,
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
                };

                lines.Add(line);

                boms.Add(product, line);

                lineCount++;
                items += qty;
                lineDiscount += discount;
                totalAmount += netAmount;
            }

            // Validate order promotions
            var (orderPromotions, opResult) = _productService.ValidatePromotions(rq.Promotions, [.. customer.Promotions, .. customer.Customer.Promotions], totalAmount);
            if (!opResult.Ok)
            {
                return opResult;
            }

            promotionSummary.Add(orderPromotions);

            // Order discount
            var orderDiscount = orderPromotions?.Sum(p => p.Amount) ?? 0;

            // Order amount without discount
            var orderAmount = totalAmount - orderDiscount;

            if (rq.Amount.HasValue && rq.Amount.Value != orderAmount)
            {
                return CreateNoValidDataResult(nameof(rq.Amount), orderAmount, rq.Amount.Value, "Order");
            }

            // BOM calculation
            var bomResult = await boms.CalculateAsync(_productService, productRQ, cancellationToken);
            if (!bomResult.Ok)
            {
                return bomResult;
            }

            var userId = rq.UserId ?? User.Oid;

            var order = new OrderHeader
            {
                CoreOrganizationId = orgId,
                UserId = userId,
                Kind = OrderKind.Order,
                SellerId = User.Pid,
                BuyerId = customerId,
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
                Promotions = orderPromotions,
                Amount = orderAmount,
                Discount = orderDiscount,
                LineDiscount = lineDiscount,
                Lines = lineCount,
                Items = items,
                TaxAmount = rq.TaxAmount.GetValueOrDefault(),
                Data = rq.Data,
                Status = rq.Status.GetValueOrDefault(),

                OrderLines = lines
            };

            // Set the order reference for each line's BOM lines
            foreach (var line in lines)
            {
                if (line.BomLines != null)
                {
                    foreach (var bomLine in line.BomLines)
                    {
                        bomLine.Order = order;
                    }
                }
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (rq.Tags?.Any() is true)
                {
                    var tagIds = await _commonService.AddTagsAsync(FeatureTagKind.Order, rq.Tags, cancellationToken);
                    order.Tags = [.. tagIds];
                }

                _db.OrderHeaders.Add(order);

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

            var id = order.Id;

            // Push message
            var message = new CreateOrderMessage
            {
                Data = User.CreateMessageData(App.AppId, id, order.Title),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.OrderCreateRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CreateOrderMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        private IQueryable<OrderHeader> CreateQuery(OrderListRQ rq, Func<IQueryable<OrderHeader>, IQueryable<OrderHeader>>? filters = null)
        {
            var query = _db.OrderAndPOs(User.OrganizationInt).AsNoTracking()
                .QueryEtsoo(rq, (o) => o.Id, (o) => o.Status, (q) =>
                {
                    if (rq.Kind.HasValue)
                    {
                        q = q.Where(p => p.Kind == rq.Kind.Value);
                    }

                    if (rq.TagId != null)
                    {
                        q = q.Where(p => p.Tags != null && p.Tags.Contains(rq.TagId.Value));
                    }

                    if (!string.IsNullOrEmpty(rq.Source))
                    {
                        q = q.Where(o => o.Source == rq.Source.ToUpper());
                    }

                    if (rq.CustomerId.HasValue)
                    {
                        q = q.Where(o => o.BuyerId == rq.CustomerId.Value);
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
        /// Duplicate test (Orders or POs)
        /// 重复测试（订单或采购）
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async ValueTask<OrderDuplicateTestData[]?> DuplicateTestAsync(OrderDuplicateTestRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;

            var q = _db.OrderAndPOs(orgId).AsNoTracking();

            if (rq.Kind.HasValue)
            {
                q = q.Where(p => p.Kind == rq.Kind.Value);
            }

            var hasFilter = false;

            if (rq.ExcludedId.HasValue)
            {
                q = q.Where(p => p.Id != rq.ExcludedId.Value);
            }

            if (!string.IsNullOrEmpty(rq.Title))
            {
                q = q.Where(p => p.Title.ToLower() == rq.Title.ToLower());
                hasFilter = true;
            }

            if (!string.IsNullOrEmpty(rq.AssignedId))
            {
                q = q.Where(p => p.AssignedId != null && p.AssignedId == rq.AssignedId.ToUpper());
                hasFilter = true;
            }

            if (!string.IsNullOrEmpty(rq.SourceId))
            {
                q = q.Where(p => p.SourceId != null && p.SourceId == rq.SourceId.ToUpper());
                hasFilter = true;
            }

            if (!hasFilter) return null;

            return await q.Select(p => new OrderDuplicateTestData
            {
                Id = p.Id,
                Title = p.Title,
                Kind = p.Kind
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// List order JSON data
        /// 订单列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(OrderListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            await _commonService.UpdateTagAsync(rq, User.OrganizationInt, cancellationToken);

            await CreateQuery(rq)
                .Select(o => new OrderListData
                {
                    Id = o.Id,
                    Title = o.Title
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List order JSON data
        /// 订单列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrderListAllData[]> ListAllAsync(OrderListAllRQ rq, CancellationToken cancellationToken = default)
        {
            await _commonService.UpdateTagAsync(rq, User.OrganizationInt, cancellationToken);

            return await CreateQuery(rq, (q) =>
            {
                if (rq.PersonId.HasValue)
                {
                    var personId = rq.PersonId.Value;
                    q = q.Where(o => o.BuyerId == personId || o.SellerId == personId);
                }

                return q;
            })
            .Select(o => new OrderListAllData
            {
                Id = o.Id,
                Title = o.Title,
                Kind = o.Kind
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Query order JSON data
        /// 查询订单JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrderQueryData[]> QueryAsync(OrderQueryRQ rq, CancellationToken cancellationToken = default)
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
            .Select(o => new OrderQueryData
            {
                Id = o.Id,
                Source = o.Source,
                Title = o.Title,
                CustomerId = o.BuyerId,
                CustomerName = o.Buyer.Name,
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
        public async Task<OrderViewData?> ReadAsync(long id, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.View, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            var order = await _db.Orders(orgId).AsNoTracking()
                 .Where(p => p.Id == id)
                 .Select(p => new OrderViewData
                 {
                     Id = p.Id,
                     Source = p.Source,
                     SourceId = p.SourceId,
                     AssignedId = p.AssignedId,
                     CustomerId = p.BuyerId,
                     CustomerName = p.Buyer.Name,
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

            if (order != null)
            {
                // Push message
                var message = new ReadOrderMessage
                {
                    Data = User.CreateMessageData(App.AppId, id, order.Title)
                };
                await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.ReadOrderMessage, cancellationToken);
            }

            return order;
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
            var result = await _db.Orders(orgId)
                .Where(p => p.Id == id && (isLocalManage || p.UserId == User.Oid) && p.Status < EntityStatus.Inactivated)
                .Select(o => new
                {
                    Order = o,
                    Lines = o.OrderLines.Where(l => l.BomId == null)
                })
                .Select(o => new
                {
                    o.Order,
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

            var order = result.Order;
            var lines = result.Lines;
            var items = result.Items;
            var lineDiscount = result.LineDiscount;
            var totalAmount = result.TotalAmount;

            // Attach the order to the context for tracking
            _db.Attach(order);

            // Check customer data
            var customerRQ = new CustomerReadForSaleRQ
            {
                CustomerId = order.BuyerId,
                Currency = order.Currency
            };

            var customer = await _customerService.ReadForSaleAsync(customerRQ, cancellationToken);
            if (customer == null || customer.Customer == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(customerRQ.CustomerId));
            }

            // Calculate order promotions
            var promotions = _productService.CalculatePromotions([.. customer.Promotions, .. customer.Customer.Promotions], totalAmount);

            // Order discount
            var orderDiscount = promotions?.Sum(p => p.Amount) ?? 0;

            // Order amount without discount
            var orderAmount = totalAmount - orderDiscount;

            // Update
            order.Lines = lines;
            order.LineDiscount = lineDiscount;
            order.Amount = orderAmount;
            order.Discount = orderDiscount;
            order.Promotions = promotions;


            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new RecalculateOrderMessage
            {
                Data = User.CreateMessageData(App.AppId, id, order.Title),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.RecalculateOrderMessage, cancellationToken);

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
        public async Task<IActionResult> UpdateAsync(OrderUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var order = await _db.Orders(orgId)
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
                    var paymentExists = await _db.OrderPayments(orgId).Where(p => p.Id == rq.PaymentId.Value && p.IsOrder).AnyAsync(cancellationToken);
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
                    var deliveryExists = await _db.OrderDeliveries(orgId).Where(d => d.Id == rq.DeliveryId.Value && d.IsOrder).AnyAsync(cancellationToken);
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
                    var addressFormatted = await _db.PersonAddresses(order.BuyerId).Where(a => a.Id == rq.AddressId.Value).Select(a => a.FormattedAddress).FirstOrDefaultAsync(cancellationToken);
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
                    var tagIds = await _commonService.AddTagsAsync(FeatureTagKind.Order, rq.Tags, cancellationToken);
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
            var message = new UpdateOrderMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, order.Title),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateOrderMessage, cancellationToken);

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
        public async Task<OrderUpdateReadData?> UpdateReadAsync(long id, CancellationToken cancellationToken = default)
        {
            // Permission check
            var (isEdit, isManage) = await CheckEditPermissionsAsync(cancellationToken);
            if (!isEdit)
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.Orders(orgId).AsNoTracking()
                .Where(p => p.Id == id && (isManage || p.UserId == User.Oid))
                .Select(p => new OrderUpdateReadData
                {
                    Id = p.Id,
                    Source = p.Source,
                    SourceId = p.SourceId,
                    CustomerId = p.BuyerId,
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