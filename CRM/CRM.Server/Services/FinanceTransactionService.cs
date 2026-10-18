using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.User;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.String;
using CRM.Server.Application;
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
                .Select(o => new { o.BuyerId, o.Title, o.Currency, o.Amount, o.PaidAmount })
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.OrderId));
            }

            var amount = rq.Amount;

            var balance = order.Amount - order.PaidAmount;
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

                var trans = new FinanceTransaction
                {
                    Kind = FinanceTransactionKind.Collect,
                    AccountId = item.AccountId,
                    Title = title,
                    OrderId = orderId,
                    Amount = item.Amount,
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
                .Select(o => new { o.SellerId, o.Title, o.Currency, o.Amount, o.PaidAmount })
                .FirstOrDefaultAsync(cancellationToken);

            if (po == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.OrderId));
            }

            var amount = rq.Amount;

            var balance = po.Amount - po.PaidAmount;
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
                    Amount = item.Amount,
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
    }
}
