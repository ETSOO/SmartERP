using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.String;
using CRM.Server.Application;
using CRM.Server.Dto.FinanceAccount;
using CRM.Server.Dto.FinanceTransaction;
using CRM.Server.Dto.Product;
using CRM.Server.RQ.FinanceTransaction;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.FinanceTransaction;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Text.Json;

namespace CRM.Server.Services
{
    /// <summary>
    /// Finance transaction service
    /// 财务交易服务
    /// </summary>
    public class FinanceTransactionService : MyUserService, IFinanceTransactionService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public FinanceTransactionService(
            MyDbContext db,
            IMyApp app,
            MyAppConfiguration config,
            CurrentUserAccessor userAccessor,
            ILogger<FinanceTransactionService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, config, userAccessor.UserSafe, "financeTransaction", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
        }

        /// <summary>
        /// Adjust account
        /// 调整账户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> AdjustAsync(FinanceTransactionAdjustRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            // Account
            var accountId = rq.AccountId;
            var account = await _db.FinanceAccounts.AsNoTracking()
                .Where(a => a.Id == accountId && a.CoreOrganizationId == orgId && a.Status < EntityStatus.Inactivated)
                .Select(a => new { a.PersonId, a.Kind, a.Balance, a.RefreshTime })
                .FirstOrDefaultAsync(cancellationToken);

            if (account == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.AccountId));
            }

            var kind = account.Kind;
            if (kind == FinanceAccountKind.Pass)
            {
                return ApplicationErrors.AccessDenied.AsResult(nameof(account.Kind));
            }

            var amount = rq.Amount;

            var transKind = account.RefreshTime.HasValue ? FinanceTransactionKind.Adjustment : FinanceTransactionKind.Init;

            var action = transKind == FinanceTransactionKind.Init ? Properties.Resources.Initialization : Properties.Resources.Adjustment;
            var title = $"{action} - {rq.Description}";

            var item = new FinanceTransaction
            {
                Kind = transKind,
                AccountId = accountId,
                Title = title,
                Amount = amount
            };

            var jsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.FinanceTransactionAdjustRQ);

            return await ProcessAsync(account.PersonId, item, jsonData, cancellationToken);
        }

        private IQueryable<FinanceTransaction> CreateQuery(FinanceTransactionListRQ rq, Func<IQueryable<FinanceTransaction>, IQueryable<FinanceTransaction>>? filters = null)
        {
            var orgId = User.OrganizationInt;

            var query = _db.FinanceTransactions.AsNoTracking()
                .Where(t => t.Account.CoreOrganizationId == orgId)
                .QueryEtsoo(rq, (t) => t.Id, null, (q) =>
                {
                    if (rq.PersonId.HasValue)
                    {
                        var personId = rq.PersonId.Value;
                        if (personId == 0) personId = User.Pid;

                        q = q.Where(t => t.Account.PersonId == personId);
                    }

                    if (!string.IsNullOrEmpty(rq.Currency))
                    {
                        q = q.Where(t => t.Account.Currency == rq.Currency.ToUpper());
                    }

                    if (rq.AccountId.HasValue)
                    {
                        q = q.Where(t => t.AccountId == rq.AccountId.Value);
                    }

                    if (rq.Kind.HasValue)
                    {
                        q = q.Where(t => t.Kind == rq.Kind.Value);
                    }
                    else if (rq.Kinds != null)
                    {
                        q = q.Where(t => rq.Kinds.Contains(t.Kind));
                    }

                    if (rq.OrderId.HasValue)
                    {
                        q = q.Where(t => t.OrderId == rq.OrderId.Value);
                    }

                    if (rq.InnerRef.HasValue)
                    {
                        q = q.Where(t => t.InnerRef == rq.InnerRef.Value);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        q = q.Where(t => EF.Functions.ILike(t.Title, $"%{keyword}%")
                            || (t.ReferenceId != null && EF.Functions.ILike(t.ReferenceId, $"%{keyword}%"))
                        );
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
        /// List finance transaction JSON data
        /// 财务交易列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<FinanceTransactionListData[]?> ListAsync(FinanceTransactionListRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.List, cancellationToken))
            {
                return null;
            }

            return await CreateQuery(rq)
                .Select(t => new FinanceTransactionListData
                {
                    Id = t.Id,
                    Kind = t.Kind,
                    Title = t.Title,
                    Amount = t.Amount,
                    Creation = t.Creation,
                    HappenDate = t.HappenDate
                }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Offset
        /// 冲减
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> OffsetAsync(FinanceTransactionOffsetRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Manage, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var id = rq.Id;
            var orgId = User.OrganizationInt;

            // Item
            var item = await _db.FinanceTransactions(orgId).AsNoTracking()
                .Where(t => t.Id == id && t.OffsetId == null)
                .Select(t => new
                {
                    t.Account.PersonId,
                    t.Kind,
                    t.AccountId,
                    t.Title,
                    t.Amount,
                    t.OrderId,
                    t.ReferenceId,
                    t.TargetPersonId,
                    t.TargetAccountId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.Id));
            }

            var title = item.Title + $" ({Properties.Resources.Offset})";

            var newItem = new FinanceTransaction
            {
                Kind = item.Kind,
                AccountId = item.AccountId,
                Title = title,
                Amount = -item.Amount,
                OrderId = item.OrderId,
                ReferenceId = item.ReferenceId,
                TargetPersonId = item.TargetPersonId,
                TargetAccountId = item.TargetAccountId,
                AuthorId = User.Oid
            };

            var personId = item.PersonId;

            var jsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.FinanceTransactionOffsetRQ);
            var result = await ProcessAsync(personId, newItem, jsonData, cancellationToken);

            if (result.Ok)
            {
                var offsetId = result.Data.Get<long>("id");
                if (offsetId.HasValue)
                {
                    await _db.FinanceTransactions.AsNoTracking()
                        .Where(t => t.Id == id)
                        .ExecuteUpdateAsync(
                            t => t.SetProperty(t => t.OffsetId, offsetId.Value),
                            cancellationToken
                        );
                }
            }

            return result;
        }

        /// <summary>
        /// Pay order
        /// 订单付款
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> PayOrderAsync(FinanceTransactionPayOrderRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Transaction, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orderId = rq.OrderId;
            var orgId = User.OrganizationInt;

            var order = await _db.Orders(orgId).AsNoTracking()
                .Where(o => o.Id == orderId && o.Status < EntityStatus.Inactivated)
                .Select(o => new { o.BuyerId, o.Title, o.Currency, o.TotalPayable, o.PaidAmount })
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.OrderId));
            }

            var amount = rq.Amount;

            var balance = order.TotalPayable - order.PaidAmount;
            if (balance <= 0 || amount > balance)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(order.PaidAmount));
            }

            // Check accounts
            var orgPersonId = User.Pid;
            var buyerId = order.BuyerId;
            var currency = order.Currency;
            var accountIds = rq.Items.Select(i => i.AccountId).Distinct().ToArray();
            var accounts = await _db.FinanceAccounts.AsNoTracking()
                .Where(a => a.CoreOrganizationId == orgId && a.Currency == currency && accountIds.Contains(a.Id) && a.Status < EntityStatus.Inactivated)
                .Select(a => new { a.Id, a.PersonId, a.Kind, a.AccountNumber, a.Balance })
                .ToArrayAsync(cancellationToken);

            if (accounts.Length != accountIds.Length)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.Items));
            }

            foreach (var account in accounts)
            {
                var kind = account.Kind;
                var isKindValid = true;
                if (kind == FinanceAccountKind.Pass || kind == FinanceAccountKind.Prepaid)
                {
                    if (account.PersonId != buyerId)
                    {
                        isKindValid = false;
                    }
                    else
                    {
                        var amountTotal = rq.Items.Where(i => i.AccountId == account.Id).Sum(i => i.Amount);
                        if (amountTotal > account.Balance)
                        {
                            var amountCheckResult = ApplicationErrors.InsufficientBalance.AsResult();
                            amountCheckResult.Detail = $"{account.Id} ({StringUtils.HideData(account.AccountNumber)})";
                            return amountCheckResult;
                        }
                    }
                }
                else if (kind == FinanceAccountKind.Cash || kind == FinanceAccountKind.Transfer || kind == FinanceAccountKind.Super)
                {
                    if (account.PersonId != orgPersonId)
                    {
                        isKindValid = false;
                    }
                }
                else
                {
                    isKindValid = false;
                }

                if (!isKindValid)
                {
                    var kindResult = ApplicationErrors.NoValidData.AsResult(nameof(account.Kind));
                    kindResult.Detail = $"{account.Id} ({StringUtils.HideData(account.AccountNumber)})";
                    return kindResult;
                }
            }

            var jsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.FinanceTransactionPayOrderRQ);

            foreach (var item in rq.Items)
            {
                var account = accounts.First(a => a.Id == item.AccountId);

                var title = $"{Properties.Resources.Collect}, {order.Title}";

                if (!string.IsNullOrEmpty(rq.Description))
                {
                    title += $" - {rq.Description}";
                }

                var oneAmount = item.Amount;
                if (account.Kind == FinanceAccountKind.Pass || account.Kind == FinanceAccountKind.Prepaid)
                {
                    oneAmount = -oneAmount;
                }

                var trans = new FinanceTransaction
                {
                    Kind = FinanceTransactionKind.Collect,
                    AccountId = item.AccountId,
                    Title = title,
                    OrderId = orderId,
                    Amount = oneAmount,
                    HappenDate = rq.HappenDate,
                    AuthorId = User.Oid
                };

                var result = await ProcessAsync(account.PersonId, trans, jsonData, cancellationToken);
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// Pay purchase order
        /// 采购付款
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> PayPOAsync(FinanceTransactionPayPORQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Transaction, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orderId = rq.OrderId;
            var orgId = User.OrganizationInt;

            var po = await _db.POs(orgId).AsNoTracking()
                .Where(o => o.Id == orderId && o.Status < EntityStatus.Inactivated)
                .Select(o => new { o.SellerId, o.Title, o.Currency, o.TotalPayable, o.PaidAmount })
                .FirstOrDefaultAsync(cancellationToken);

            if (po == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.OrderId));
            }

            var amount = rq.Amount;

            var balance = po.TotalPayable - po.PaidAmount;
            if (balance <= 0 || amount > balance)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(po.PaidAmount));
            }

            // Check accounts
            var orgPersonId = User.Pid;
            var sellerId = po.SellerId;
            var currency = po.Currency;
            var accountIds = rq.Items.Select(i => i.AccountId).Distinct().ToArray();
            var accounts = await _db.FinanceAccounts.AsNoTracking()
                .Where(a => a.CoreOrganizationId == orgId && a.PersonId == orgPersonId && a.Currency == currency && accountIds.Contains(a.Id) && a.Status < EntityStatus.Inactivated)
                .Select(a => new { a.Id, a.Kind, a.AccountNumber, a.Balance })
                .ToArrayAsync(cancellationToken);

            if (accounts.Length != accountIds.Length)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.Items));
            }

            foreach (var account in accounts)
            {
                var kind = account.Kind;
                var isKindValid = true;
                if (kind == FinanceAccountKind.Pass
                    || kind == FinanceAccountKind.Prepaid
                    || kind == FinanceAccountKind.Cash
                    || kind == FinanceAccountKind.Transfer
                    || kind == FinanceAccountKind.Super)
                {
                    var amountTotal = rq.Items.Where(i => i.AccountId == account.Id).Sum(i => i.Amount);
                    if (amountTotal > account.Balance)
                    {
                        var amountCheckResult = ApplicationErrors.InsufficientBalance.AsResult();
                        amountCheckResult.Detail = $"{account.Id} ({StringUtils.HideData(account.AccountNumber)})";
                        return amountCheckResult;
                    }
                }
                else
                {
                    isKindValid = false;
                }

                if (!isKindValid)
                {
                    var kindResult = ApplicationErrors.NoValidData.AsResult(nameof(account.Kind));
                    kindResult.Detail = $"{account.Id} ({StringUtils.HideData(account.AccountNumber)})";
                    return kindResult;
                }
            }

            var jsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.FinanceTransactionPayPORQ);

            foreach (var item in rq.Items)
            {
                var account = accounts.First(a => a.Id == item.AccountId);

                var title = $"{Properties.Resources.Payment}, {po.Title}";

                if (!string.IsNullOrEmpty(rq.Description))
                {
                    title += $" - {rq.Description}";
                }

                var trans = new FinanceTransaction
                {
                    Kind = FinanceTransactionKind.Payment,
                    AccountId = item.AccountId,
                    Title = title,
                    OrderId = orderId,
                    Amount = -item.Amount,
                    HappenDate = rq.HappenDate,
                    AuthorId = User.Oid
                };

                var result = await ProcessAsync(orgPersonId, trans, jsonData, cancellationToken);
            }

            return ActionResult.Success;
        }

        /// <summary>
        /// Process the transaction
        /// </summary>
        /// <param name="personId">Source account person id</param>
        /// <param name="item">Transaction item</param>
        /// <param name="jsonData">Request JSON data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        private async Task<IActionResult> ProcessAsync(long personId, FinanceTransaction item, string jsonData, CancellationToken cancellationToken)
        {
            if (item.AuthorId == 0)
            {
                item.AuthorId = User.Oid;
            }

            if (item.TargetAccountId.HasValue && item.InnerRef == null)
            {
                item.InnerRef = Guid.NewGuid();
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Add the item
                _db.FinanceTransactions.Add(item);

                // Account
                var accountId = item.AccountId;
                var amount = item.Amount;

                await _db.FinanceAccounts
                    .Where(a => a.Id == accountId)
                    .ExecuteUpdateAsync(
                        a => a.SetProperty(a => a.Balance, a => a.Balance + amount)
                            .SetProperty(a => a.RefreshTime, a => DateTimeOffset.UtcNow)
                        , cancellationToken
                    );

                // Order
                if (item.OrderId.HasValue)
                {
                    var orderId = item.OrderId.Value;
                    await _db.OrderHeaders
                        .Where(o => o.Id == orderId)
                        .ExecuteUpdateAsync(
                            o => o.SetProperty(o => o.PaidAmount, o => o.PaidAmount + amount),
                            cancellationToken
                        );
                }

                // Target account
                if (item.TargetAccountId.HasValue)
                {
                    var targetAccountId = item.TargetAccountId.Value;

                    // Add target item
                    var targetItem = new FinanceTransaction
                    {
                        Kind = item.Kind,
                        AccountId = targetAccountId,
                        Title = item.Title,
                        Amount = -amount,
                        OrderId = item.OrderId,
                        ReferenceId = item.ReferenceId,
                        TargetPersonId = personId,
                        TargetAccountId = accountId,
                        AuthorId = item.AuthorId,
                        InnerRef = item.InnerRef
                    };
                    _db.FinanceTransactions.Add(targetItem);

                    await _db.FinanceAccounts
                        .Where(a => a.Id == targetAccountId)
                        .ExecuteUpdateAsync(
                            a => a.SetProperty(a => a.Balance, a => a.Balance - amount)
                                 .SetProperty(a => a.RefreshTime, a => DateTimeOffset.UtcNow)
                            , cancellationToken
                        );
                }

                // Submit
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

            var id = item.Id;

            // Push message
            var message = new ProcessFinanceTransactionMessage
            {
                Data = User.CreateMessageData(App.AppId, id, item.Title),
                JsonData = jsonData
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.ProcessFinanceTransactionMessage, cancellationToken);

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Query finance transactions
        /// 查询财务交易
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<FinanceTransactionQueryData[]?> QueryAsync(FinanceTransactionQueryRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Query, cancellationToken))
            {
                return null;
            }

            return await CreateQuery(rq, (q) =>
            {
                if (rq.CreationStart.HasValue)
                {
                    q = q.Where(t => t.Creation >= rq.CreationStart.Value);
                }

                if (rq.CreationEnd.HasValue)
                {
                    q = q.Where(t => t.Creation <= rq.CreationEnd.Value);
                }

                if (rq.HappenDateStart.HasValue)
                {
                    q = q.Where(t => t.HappenDate >= rq.HappenDateStart.Value);
                }

                if (rq.HappenDateEnd.HasValue)
                {
                    q = q.Where(t => t.HappenDate <= rq.HappenDateEnd.Value);
                }

                return q;
            })
            .Select(t => new FinanceTransactionQueryData
            {
                Id = t.Id,
                Kind = t.Kind,
                Title = t.Title,
                Amount = t.Amount,
                ReferenceId = t.ReferenceId,
                OrderId = t.OrderId,
                TargetAccountId = t.TargetAccountId,
                Creation = t.Creation,
                HappenDate = t.HappenDate,
                AuthorName = t.Author.Name
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Reimburse
        /// 报销
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> ReimburseAsync(FinanceTransactionReimburseRQ rq, CancellationToken cancellationToken = default)
        {
            return ActionResult.Success;
        }

        /// <summary>
        /// Transfer
        /// 转账
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> TransferAsync(FinanceTransactionTransferRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Transaction, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var sourceAccountId = rq.SourceAccountId;
            var destinationAccountId = rq.DestinationAccountId;

            var orgId = User.OrganizationInt;

            var accounts = await _db.FinanceAccounts.AsNoTracking()
                .Where(a => a.CoreOrganizationId == orgId && (a.Id == sourceAccountId || a.Id == destinationAccountId) && a.Status < EntityStatus.Inactivated)
                .Select(a => new { a.Id, a.PersonId, a.Kind, a.Currency, a.Balance, a.AccountNumber })
                .ToArrayAsync(cancellationToken);

            var sourceAccount = accounts.FirstOrDefault(a => a.Id == sourceAccountId);
            if (sourceAccount == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.SourceAccountId));
            }

            var amount = rq.Amount;
            if (sourceAccount.Balance < amount)
            {
                return ApplicationErrors.InsufficientBalance.AsResult();
            }

            var destinationAccount = accounts.FirstOrDefault(a => a.Id == destinationAccountId);
            if (destinationAccount == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.DestinationAccountId));
            }

            // Same currency
            if (sourceAccount.Currency != destinationAccount.Currency)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(destinationAccount.Currency));
            }

            var sKind = sourceAccount.Kind;
            var dKind = destinationAccount.Kind;
            if (sKind != dKind)
            {
                if (sKind == FinanceAccountKind.Cash && dKind != FinanceAccountKind.Transfer && dKind != FinanceAccountKind.Super)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(destinationAccount.Kind));
                }
                else if (sKind == FinanceAccountKind.Transfer && dKind != FinanceAccountKind.Super)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(destinationAccount.Kind));
                }
                else if (sKind == FinanceAccountKind.Super && dKind != FinanceAccountKind.Cash && dKind != FinanceAccountKind.Transfer)
                {
                    return ApplicationErrors.NoValidData.AsResult(nameof(destinationAccount.Kind));
                }
            }

            var title = $"{Properties.Resources.Transfer} ({StringUtils.HideData(sourceAccount.AccountNumber)} -> {StringUtils.HideData(destinationAccount.AccountNumber)}) - {rq.Description}";

            var item = new FinanceTransaction
            {
                Kind = FinanceTransactionKind.Transfer,
                AccountId = sourceAccountId,
                Title = title,
                Amount = -amount,
                ReferenceId = rq.ReferenceId,
                TargetPersonId = destinationAccount.PersonId,
                TargetAccountId = destinationAccountId,
                AuthorId = User.Oid
            };

            var jsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.FinanceTransactionTransferRQ);

            return await ProcessAsync(sourceAccount.PersonId, item, jsonData, cancellationToken);
        }

        /// <summary>
        /// View order data for collection
        /// 浏览订单数据用于收款
        /// </summary>
        /// <param name="id">Order id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Data</returns>
        public async Task<FinanceTransactionViewOrderData?> ViewOrderAsync(long id, CancellationToken cancellationToken)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Transaction, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            var order = await _db.Orders(orgId).AsNoTracking()
                .Where(o => o.Id == id && o.Status < EntityStatus.Inactivated)
                .Select(o => new FinanceTransactionOrderViewMainData
                {
                    Id = o.Id,
                    CustomerId = o.BuyerId,
                    CustomerName = o.Buyer.Name,
                    Title = o.Title,
                    Payment = o.Payment == null ? null : o.Payment.Title,
                    PaymentInstruction = o.PaymentInstruction,
                    Currency = o.Currency,
                    TotalPayable = o.TotalPayable,
                    PaidAmount = o.PaidAmount,
                    Creation = o.Creation,

                    Products = o.OrderLines.GroupBy(ol => ol.ProductId).Select(g => new ProductBomItem
                    {
                        ProductId = g.Key,
                        Qty = g.Sum(ol => ol.Qty)
                    })
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                return null;
            }

            var currency = order.Currency;
            var productIds = order.Products.Select(p => p.ProductId).ToArray();

            var orgPersonId = User.Pid;
            var orgAccounts = await _db.FinanceAccounts.AsNoTracking()
                .Where(a => a.CoreOrganizationId == orgId
                        && a.PersonId == orgPersonId
                        && a.Currency == currency
                        && a.Status < EntityStatus.Inactivated
                        && (a.Kind == FinanceAccountKind.Cash || a.Kind == FinanceAccountKind.Transfer || a.Kind == FinanceAccountKind.Super)
                        && (a.ProductId == null || productIds.Contains(a.ProductId.Value))
                )
                .Select(a => new FinanceAccountItem
                {
                    Id = a.Id,
                    Kind = a.Kind,
                    Bank = a.Bank,
                    AccountNumber = a.AccountNumber,
                    ProductId = a.ProductId
                })
                .ToArrayAsync(cancellationToken);

            var personId = order.CustomerId;
            var accounts = await _db.FinanceAccounts.AsNoTracking()
                .Where(a => a.CoreOrganizationId == orgId
                        && a.PersonId == personId
                        && a.Currency == currency
                        && a.Status < EntityStatus.Inactivated
                        && (a.Kind == FinanceAccountKind.Prepaid || a.Kind == FinanceAccountKind.Pass)
                        && a.Balance > 0
                        && (a.ProductId == null || productIds.Contains(a.ProductId.Value))
                )
                .Select(a => new FinanceAccountItemWithBalance
                {
                    Id = a.Id,
                    Kind = a.Kind,
                    Bank = a.Bank,
                    AccountNumber = a.AccountNumber,
                    ProductId = a.ProductId,
                    Balance = a.Balance
                })
                .ToArrayAsync(cancellationToken);

            return new FinanceTransactionViewOrderData
            {
                Order = order,
                OrgAccounts = orgAccounts,
                Accounts = accounts
            };
        }

        /// <summary>
        /// View purchase order data for payment
        /// 浏览采购订单数据用于付款
        /// </summary>
        /// <param name="id">Purchase order id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Data</returns>
        public async Task<FinanceTransactionViewPOData?> ViewPOAsync(long id, CancellationToken cancellationToken)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Transaction, cancellationToken))
            {
                return null;
            }

            var orgId = User.OrganizationInt;

            var po = await _db.POs(orgId).AsNoTracking()
                .Where(o => o.Id == id && o.Status < EntityStatus.Inactivated)
                .Select(o => new FinanceTransactionPOViewMainData
                {
                    Id = o.Id,
                    SupplierId = o.SellerId,
                    SupplierName = o.Seller.Name,
                    Title = o.Title,
                    Payment = o.Payment == null ? null : o.Payment.Title,
                    PaymentInstruction = o.PaymentInstruction,
                    Currency = o.Currency,
                    TotalPayable = o.TotalPayable,
                    PaidAmount = o.PaidAmount,
                    Creation = o.Creation,

                    Products = o.OrderLines.GroupBy(ol => ol.ProductId).Select(g => new ProductBomItem
                    {
                        ProductId = g.Key,
                        Qty = g.Sum(ol => ol.Qty)
                    })
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (po == null)
            {
                return null;
            }

            var currency = po.Currency;
            var productIds = po.Products.Select(p => p.ProductId).ToArray();

            var orgPersonId = User.Pid;
            var orgAccounts = await _db.FinanceAccounts.AsNoTracking()
                .Where(a => a.CoreOrganizationId == orgId
                        && a.PersonId == orgPersonId
                        && a.Currency == currency
                        && a.Status < EntityStatus.Inactivated
                        && (a.Kind == FinanceAccountKind.Cash || a.Kind == FinanceAccountKind.Transfer || a.Kind == FinanceAccountKind.Super)
                        && a.Balance > 0
                        && (a.ProductId == null || productIds.Contains(a.ProductId.Value))
                )
                .Select(a => new FinanceAccountItem
                {
                    Id = a.Id,
                    Kind = a.Kind,
                    Bank = a.Bank,
                    AccountNumber = a.AccountNumber,
                    ProductId = a.ProductId
                })
                .ToArrayAsync(cancellationToken);

            var personId = po.SupplierId;
            var accounts = await _db.FinanceAccounts.AsNoTracking()
                .Where(a => a.CoreOrganizationId == orgId
                        && a.PersonId == personId
                        && a.Currency == currency
                        && a.Status < EntityStatus.Inactivated
                        && (a.Kind == FinanceAccountKind.Prepaid || a.Kind == FinanceAccountKind.Pass)
                        && a.Balance > 0
                        && (a.ProductId == null || productIds.Contains(a.ProductId.Value))
                )
                .Select(a => new FinanceAccountItemWithBalance
                {
                    Id = a.Id,
                    Kind = a.Kind,
                    Bank = a.Bank,
                    AccountNumber = a.AccountNumber,
                    ProductId = a.ProductId,
                    Balance = a.Balance
                })
                .ToArrayAsync(cancellationToken);

            return new FinanceTransactionViewPOData
            {
                Order = po,
                OrgAccounts = orgAccounts,
                Accounts = accounts
            };
        }
    }
}
