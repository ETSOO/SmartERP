using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.User;
using com.etsoo.Utils.Actions;
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
                .Where(a => a.Id == accountId && a.Person.OrgId == orgId && a.Status < EntityStatus.Inactivated)
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
                    t.CoreOrganizationId,
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
                CoreOrganizationId = item.CoreOrganizationId,
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
                .Where(o => o.Id == orderId)
                .Select(o => new { o.Amount, o.PaidAmount })
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.OrderId));
            }

            return ActionResult.Success;
        }

        private async Task<IActionResult> ProcessAsync(long personId, FinanceTransaction item, string jsonData, CancellationToken cancellationToken)
        {
            if (item.AuthorId == 0)
            {
                item.AuthorId = User.Oid;
            }

            if (item.CoreOrganizationId == 0)
            {
                item.CoreOrganizationId = User.OrganizationInt;
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
    }
}
