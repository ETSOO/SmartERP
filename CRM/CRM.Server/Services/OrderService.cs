using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.String;
using CRM.Server.Dto.Order;
using CRM.Server.Dto.Product;
using CRM.Server.Dto.System;
using CRM.Server.RQ.Customer;
using CRM.Server.RQ.Order;
using CRM.Server.RQ.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using PlatformShared.Extentions;
using System.Buffers;

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

        public OrderService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<OrderService> logger,
            ICommonService commonService,
            ICustomerService customerService,
            IProductService productService
        )
            : base(app, userAccessor.UserSafe, "order", logger)
        {
            _db = db;
            _commonService = commonService;
            _customerService = customerService;
            _productService = productService;
        }

        (IEnumerable<PromotionSaleItem>? saleItems, IActionResult result) ValidatePromotions(IEnumerable<PromotionSaleItemBase>? items, IEnumerable<PromotionItem> promotions, decimal amount, IPromotionCodeLine? sale = null)
        {
            if (items == null)
            {
                return (null, ActionResult.Success);
            }

            var saleItems = new List<PromotionSaleItem>();

            // Non-stackable items
            var np = promotions.Where(p => !p.Stackable)
                .Select(p => PromotionCode.TryParse<PromotionCode>(p.Code, out var code) ? code.Calculate(p, sale, amount) : null)
                .OrderByDescending(p => p?.Amount).FirstOrDefault();

            if (np != null)
            {
                saleItems.Add(np);
                amount -= np.Amount;
            }

            // Stackable items
            foreach (var p in promotions.Where(p => p.Stackable))
            {
                if (PromotionCode.TryParse<PromotionCode>(p.Code, out var code))
                {
                    var result = code.Calculate(p, sale, amount);
                    if (result != null)
                    {
                        amount -= result.Amount;
                        saleItems.Add(result);
                    }
                }
            }

            // Validate with items (user side trust)
            foreach (var item in items)
            {
                var saleItem = saleItems.FirstOrDefault(s => s.Id == item.Id);
                if (saleItem == null)
                {
                    var result = ApplicationErrors.DataOutdated.AsResult(nameof(item.Id));
                    result.Detail = item.Amount.ToString();
                    return (null, result);
                }
                else if (saleItem.Amount != item.Amount)
                {
                    var result = ApplicationErrors.DataOutdated.AsResult(nameof(item.Amount));
                    result.Detail = $"{saleItem.Title}|{saleItem.Amount}|{item.Amount}";
                    return (null, result);
                }
            }

            return (saleItems, ActionResult.Success);
        }

        IActionResult CreateNoValidDataResult(string field, decimal targetValue, decimal currentValue, string product)
        {
            var result = ApplicationErrors.NoValidData.AsResult(field);
            result.Detail = $"{targetValue}|{currentValue}|{product}";
            return result;
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

            var products = await _productService.QueryForSaleAsync(productRQ, cancellationToken);
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
                var deliveryExists = await _db.OrderDeliveries(orgId).Where(d => d.Id == rq.DeliveryId.Value).AnyAsync(cancellationToken);
                if (!deliveryExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.DeliveryId));
                }
            }

            if (rq.PaymentId.HasValue)
            {
                var paymentExists = await _db.OrderPayments(orgId).Where(p => p.Id == rq.PaymentId.Value).AnyAsync(cancellationToken);
                if (!paymentExists)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.PaymentId));
                }
            }

            // Address
            if (rq.AddressId.HasValue)
            {
                var addressExists = await _db.PersonAddresses(customerId).Where(a => a.Id == rq.AddressId.Value).AnyAsync(cancellationToken);
                if (!addressExists)
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

                // MinQty / 起订量
                if (product.MinQty.HasValue && qty < product.MinQty.Value)
                {
                    return CreateNoValidDataResult(nameof(product.MinQty), product.MinQty.Value, qty, product.Name);
                }

                // StepQty / 最小单位量
                if (product.StepQty.HasValue && qty % product.StepQty.Value != 0)
                {
                    return CreateNoValidDataResult(nameof(product.StepQty), product.StepQty.Value, qty, product.Name);
                }

                // CapQty / 购买上限
                if (product.CapQty.HasValue && qty > product.CapQty.Value)
                {
                    return CreateNoValidDataResult(nameof(product.CapQty), product.CapQty.Value, qty, product.Name);
                }

                var price = Math.Min(Math.Min(product.RetailPrice, product.PromotionPrice.GetValueOrDefault(Decimal.MaxValue)), product.CustomerRetailPrice.GetValueOrDefault(Decimal.MaxValue));

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

                var (linePromotions, lineResult) = ValidatePromotions(l.Promotions, product.Promotions, amount, sale);
                if (!lineResult.Ok)
                {
                    return lineResult;
                }

                var lineTitle = l.Title ?? product.Name;
                var discount = l.Promotions?.Sum(p => p.Amount) ?? 0;
                var netAmount = amount - discount;

                lines.Add(new OrderLine
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
                });

                lineCount++;
                items += qty;
                lineDiscount += discount;
                totalAmount += netAmount;
            }

            // Validate order promotions
            var (orderPromotions, opResult) = ValidatePromotions(rq.Promotions, [.. customer.Promotions, .. customer.Customer.Promotions], totalAmount);
            if (!opResult.Ok)
            {
                return opResult;
            }

            // Order discount
            var orderDiscount = orderPromotions?.Sum(p => p.Amount) ?? 0;

            // Order amount without discount
            var orderAmount = totalAmount - orderDiscount;

            if (rq.Amount.HasValue && rq.Amount.Value != orderAmount)
            {
                return CreateNoValidDataResult(nameof(rq.Amount), orderAmount, rq.Amount.Value, "Order");
            }

            var userId = rq.UserId ?? User.Oid;

            var order = new OrderHeader
            {
                CoreOrganizationId = orgId,
                UserId = userId,
                IsOrder = true,
                SellerId = User.Pid,
                BuyerId = rq.CustomerId,
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

            if (rq.Tags?.Any() is true)
            {
                var tagIds = await _commonService.AddTagsAsync(FeatureTagKind.Order, rq.Tags, cancellationToken);
                order.Tags = [.. tagIds];
            }

            _db.OrderHeaders.Add(order);

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            return ActionResult.Succeed(order.Id);
        }

        private IQueryable<OrderHeader> CreateQuery(OrderListRQ rq, Func<IQueryable<OrderHeader>, IQueryable<OrderHeader>>? filters = null)
        {
            var query = _db.Orders(User.OrganizationInt).AsNoTracking()
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

                    if (rq.CustomerId.HasValue)
                    {
                        q = q.Where(o => o.BuyerId == rq.CustomerId.Value);
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
                IsOrder = p.IsOrder
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
        /// Query order JSON data
        /// 查询订单JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<OrderQueryData[]> QueryAsync(OrderQueryRQ rq, CancellationToken cancellationToken = default)
        {
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
        public async Task<OrderViewData?> ReadAsync(int id, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.View, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.Orders(orgId).AsNoTracking()
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
                     Lines = p.Lines,
                     Items = p.Items,
                     Promotions = p.Promotions,
                     Culture = p.Culture,
                     Payment = p.Payment == null ? null : p.Payment.Title,
                     PaymentInstruction = p.PaymentInstruction,
                     Delivery = p.Delivery == null ? null : p.Delivery.Title,
                     DeliveryInstruction = p.DeliveryInstruction,
                     Address = p.Address == null ? null : p.Address.FormattedAddress,
                     Contact = p.Contact == null ? null : p.Contact.Name,
                     ContactId = p.ContactId,
                     Creation = p.Creation,
                     Status = p.Status,
                     Tags = p.Tags == null ? null : _db.FeatureTags.Where(k => k.CoreOrganizationId == orgId && p.Tags.Contains(k.Id)).OrderByDescending(t => t.Total).ThenBy(t => t.Tag).Select(k => k.Tag).ToList(),
                 }).FirstOrDefaultAsync(cancellationToken);
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
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var order = await _db.Orders(orgId)
                .Where(p => p.Id == rq.Id)
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
                    var paymentExists = await _db.OrderPayments(orgId).Where(p => p.Id == rq.PaymentId.Value).AnyAsync(cancellationToken);
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
                    var deliveryExists = await _db.OrderDeliveries(orgId).Where(d => d.Id == rq.DeliveryId.Value).AnyAsync(cancellationToken);
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
                    var addressExists = await _db.PersonAddresses(order.BuyerId).Where(a => a.Id == rq.AddressId.Value).AnyAsync(cancellationToken);
                    if (!addressExists)
                    {
                        return ApplicationErrors.NoId.AsResult(nameof(rq.AddressId));
                    }
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

            // Save
            await _db.SaveChangesAsync(cancellationToken);

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
            if (!await _commonService.HasPermissionAsync((short)Permissions.Order.Edit, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            return await _db.Orders(orgId).AsNoTracking()
                .Where(p => p.Id == id)
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