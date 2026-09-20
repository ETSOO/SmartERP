using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using CRM.Server.Application;
using CRM.Server.Dto.FinanceAccount;
using CRM.Server.RQ.FinanceAccount;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PlatformShared.CrmMessages;
using PlatformShared.CrmMessages.FinanceAccount;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using System.Buffers;
using System.Text.Json;

namespace CRM.Server.Services
{
    /// <summary>
    /// Finance account service
    /// 财务账户服务
    /// </summary>
    public class FinanceAccountService : MyUserService, IFinanceAccountService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;
        readonly IQueueService _queueService;

        public FinanceAccountService(
            MyDbContext db,
            IMyApp app,
            MyAppConfiguration config,
            CurrentUserAccessor userAccessor,
            ILogger<FinanceAccountService> logger,
            ICommonService commonService,
            IQueueService queueService
        )
            : base(app, config, userAccessor.UserSafe, "financeAccount", logger)
        {
            _db = db;
            _commonService = commonService;
            _queueService = queueService;
        }

        /// <summary>
        /// Create
        /// 创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateAsync(FinanceAccountCreateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Add, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            // Organization id
            var orgId = User.OrganizationInt;

            var kind = rq.Kind;

            // Owner exists
            var personId = rq.PersonId;
            var hasOwner = await _db.Persons(orgId).Where(p => p.Id == personId).AnyAsync(cancellationToken);

            if (!hasOwner)
            {
                return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
            }

            // Account exists
            var accountNumber = rq.AccountNumber.ToUpper();
            var currency = rq.Currency.ToUpper();
            var hasAccount = await _db.FinanceAccounts.Where(a => a.Person.OrgId == orgId
                && a.Kind == kind
                && a.Currency == currency
                && a.AccountNumber == accountNumber).AnyAsync(cancellationToken);

            if (hasAccount)
            {
                return ApplicationErrors.ItemExists.AsResult(nameof(rq.AccountNumber));
            }

            var productId = rq.ProductId;
            if (productId.HasValue)
            {
                var hasProduct = await _db.Products(orgId).Where(p => p.Id == productId.Value).AnyAsync(cancellationToken);
                if (!hasProduct)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.ProductId));
                }
            }
            else if (kind == FinanceAccountKind.Pass)
            {
                // For pass account, product id is required
                // 次卡必须申明产品编号
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.ProductId));
            }

            var account = new FinanceAccount
            {
                PersonId = personId,
                Kind = kind,
                Bank = rq.Bank,
                Currency = currency,
                AccountNumber = accountNumber,
                Swift = rq.Swift,
                Description = rq.Description,
                Expiry = rq.Expiry,
                ProductId = productId,
                Status = rq.Status ?? EntityStatus.Normal
            };

            // Add
            _db.FinanceAccounts.Add(account);

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            var id = account.Id;

            // Push message
            var message = new CreateFinanceAccountMessage
            {
                Data = User.CreateMessageData(App.AppId, id, $"{account.Bank} - {account.AccountNumber}"),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.FinanceAccountCreateRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.CreateFinanceAccountMessage, cancellationToken);

            // Return
            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// Bulk create
        /// 批量创建
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateBulkAsync(FinanceAccountCreateBulkRQ rq, CancellationToken cancellationToken = default)
        {
            var orgId = User.OrganizationInt;
            var orgPersonId = User.Pid;

            long personId;
            if (rq.PersonId.HasValue)
            {
                personId = rq.PersonId.Value;

                var hasPerson = await _db.Persons(orgId).Where(p => p.Id == personId).AnyAsync(cancellationToken);
                if (!hasPerson)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
                }
            }
            else
            {
                personId = orgPersonId;
            }

            var productId = rq.ProductId;
            if (productId.HasValue)
            {
                var hasProduct = await _db.Products(orgId).Where(p => p.Id == productId.Value).AnyAsync(cancellationToken);
                if (!hasProduct)
                {
                    return ApplicationErrors.NoId.AsResult(nameof(rq.ProductId));
                }
            }

            var kind = rq.Kind;
            var currency = rq.Currency;
            var bank = rq.Bank;
            var description = rq.Description;

            var amount = rq.Amount;
            var hasAmount = amount != 0;

            var times = rq.Times;

            var title = Properties.Resources.Initialization;

            var len = rq.Length - rq.Prefix.Length;
            var accounts = Enumerable.Range(0, rq.Count)
                .Select(i =>
                {
                    var currentNumber = rq.StartNumber + i;

                    var accountNo = $"{rq.Prefix}{currentNumber.ToString().PadLeft(len, '0')}";

                    return new FinanceAccount
                    {
                        PersonId = personId,
                        Kind = kind,
                        Bank = bank,
                        Currency = currency,
                        AccountNumber = accountNo,
                        Description = description,
                        Balance = amount,
                        Times = times,
                        ProductId = productId
                    };
                })
                .ToList();

            using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var bulkConfig = new BulkConfig
                {
                    PropertiesToIncludeOnUpdate = [],
                    SetOutputIdentity = hasAmount
                };

                await _db.BulkInsertOrUpdateAsync(accounts, bulkConfig, cancellationToken: cancellationToken);

                if (hasAmount)
                {
                    var transactions = accounts.Where(a => a.Id > 0).Select(a => new FinanceTransaction
                    {
                        Kind = FinanceTransactionKind.Init,
                        AccountId = a.Id,
                        Title = title,
                        Amount = a.Balance,
                        AuthorId = User.Oid,
                        Times = a.Times
                    }).ToList();

                    await _db.BulkInsertAsync(transactions, cancellationToken: cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                // Log
                return LogException(ex);
            }

            // Push message
            var logTitle = $"{title}, {accounts.First().AccountNumber}, {rq.Count}";
            var message = new BulkCreateFinanceAccountMessage
            {
                Data = User.CreateMessageData(App.AppId, orgPersonId, logTitle),
                JsonData = JsonSerializer.Serialize(rq, MyJsonSerializerContext.Default.FinanceAccountCreateBulkRQ)
            };
            await _queueService.PushAsync(message, CrmJsonSerializerContext.Default.BulkCreateFinanceAccountMessage, cancellationToken);

            return ActionResult.Success;
        }

        /// <summary>
        /// Create cash account
        /// 创建现金账户
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public Task<IActionResult> CreateCashAsync(FinanceAccountCreateCashRQ rq, CancellationToken cancellationToken = default)
        {
            // Organization's person id
            var personId = User.Pid;

            var currency = rq.Currency.ToUpper();

            var createRq = new FinanceAccountCreateRQ
            {
                PersonId = personId,
                Kind = FinanceAccountKind.Cash,
                Bank = "CASH",
                Currency = currency,
                AccountNumber = currency
            };

            return CreateAsync(createRq, cancellationToken);
        }

        private IQueryable<FinanceAccount> CreateQuery(FinanceAccountListRQ rq, Func<IQueryable<FinanceAccount>, IQueryable<FinanceAccount>>? filters = null)
        {
            var orgId = User.OrganizationInt;

            var query = _db.FinanceAccounts.AsNoTracking()
                .Where(a => a.Person.OrgId == orgId)
                .QueryEtsoo(rq, (a) => a.Id, (a) => a.Status, (q) =>
                {
                    if (rq.PersonId.HasValue)
                    {
                        var personId = rq.PersonId.Value;
                        if (personId == 0) personId = User.Pid;

                        q = q.Where(a => a.PersonId == personId);
                    }

                    if (rq.Kind.HasValue)
                    {
                        q = q.Where(a => a.Kind == rq.Kind.Value);
                    }

                    if (!string.IsNullOrEmpty(rq.Currency))
                    {
                        q = q.Where(a => a.Currency == rq.Currency.ToUpper());
                    }

                    if (rq.ProductId.HasValue)
                    {
                        q = q.Where(a => a.ProductId == rq.ProductId.Value);
                    }
                    else if (rq.ProductIdOrDefault.HasValue)
                    {
                        var productId = rq.ProductIdOrDefault.Value;
                        q = q.Where(a => a.ProductId == productId || a.ProductId == null);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        q = q.Where(a => EF.Functions.ILike(a.AccountNumber, $"%{keyword}%")
                        || EF.Functions.ILike(a.Bank, $"%{keyword}%")
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
        /// Delete
        /// 删除
        /// </summary>
        /// <param name="id">Account id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Delete, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var hasTransaction = await _db.FinanceTransactions.AsNoTracking()
                .Where(t => t.AccountId == id)
                .AnyAsync(cancellationToken);

            if (hasTransaction)
            {
                return ApplicationErrors.DeleteReferencedData.AsResult();
            }

            var result = await _db.FinanceAccounts.AsNoTracking()
                .Where(a => a.Id == id && a.Person.OrgId == orgId)
                .ExecuteDeleteAsync(cancellationToken);

            if (result == 0)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            return ActionResult.Succeed(id);
        }

        /// <summary>
        /// List finance account JSON data
        /// 财务账户列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<FinanceAccountListData[]?> ListAsync(FinanceAccountListRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.List, cancellationToken))
            {
                return null;
            }

            return await CreateQuery(rq)
                .Select(a => new FinanceAccountListData
                {
                    Id = a.Id,
                    Kind = a.Kind,
                    Bank = a.Bank,
                    Currency = a.Currency,
                    AccountNumber = a.AccountNumber
                }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Query finance account JSON data
        /// 查询财务账户JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<FinanceAccountQueryData[]?> QueryAsync(FinanceAccountQueryRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Query, cancellationToken))
            {
                return null;
            }

            return await CreateQuery(rq, (q) =>
            {
                if (rq.TimesStart.HasValue)
                {
                    q = q.Where(a => a.Times >= rq.TimesStart.Value);
                }

                if (rq.TimesEnd.HasValue)
                {
                    q = q.Where(a => a.Times < rq.TimesEnd.Value);
                }

                return q;
            })
            .Select(a => new FinanceAccountQueryData
            {
                Id = a.Id,
                PersonId = a.PersonId,
                PersonName = a.Person.Name,
                Kind = a.Kind,
                Bank = a.Bank,
                Currency = a.Currency,
                AccountNumber = a.AccountNumber,
                Description = a.Description,
                Times = a.Times,
                Status = a.Status,
                Expiry = a.Expiry
            }).ToArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(FinanceAccountUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Edit, cancellationToken))
            {
                return ApplicationErrors.AccessDenied.AsResult();
            }

            var orgId = User.OrganizationInt;

            var account = await _db.FinanceAccounts.Where(a => a.Id == rq.Id && a.Person.OrgId == orgId).FirstOrDefaultAsync(cancellationToken);
            if (account == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var isCash = account.Kind == FinanceAccountKind.Cash;

            var accountChanged = false;

            if (!isCash)
            {
                var personId = rq.PersonId;
                if (rq.IsModified(nameof(rq.PersonId)) && personId.HasValue)
                {
                    var hasOwner = await _db.Persons(orgId).Where(p => p.Id == personId).AnyAsync(cancellationToken);

                    if (!hasOwner)
                    {
                        return ApplicationErrors.NoId.AsResult(nameof(rq.PersonId));
                    }

                    account.PersonId = personId.Value;
                }

                if (rq.IsModified(nameof(rq.Kind)) && rq.Kind.HasValue)
                {
                    account.Kind = rq.Kind.Value;
                    accountChanged = true;
                }

                if (rq.IsModified(nameof(rq.Bank)) && !string.IsNullOrEmpty(rq.Bank))
                {
                    account.Bank = rq.Bank;
                }

                if (rq.IsModified(nameof(rq.Currency)) && !string.IsNullOrEmpty(rq.Currency))
                {
                    account.Currency = rq.Currency.ToUpper();
                    accountChanged = true;
                }

                if (rq.IsModified(nameof(rq.Swift)))
                {
                    account.Swift = rq.Swift;
                }
            }

            if (rq.IsModified(nameof(rq.AccountNumber)) && !string.IsNullOrEmpty(rq.AccountNumber))
            {
                account.AccountNumber = rq.AccountNumber.ToUpper();
                accountChanged = true;
            }

            if (rq.IsModified(nameof(rq.Description)))
            {
                account.Description = rq.Description;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                account.Status = rq.Status.Value;
            }

            if (rq.IsModified(nameof(rq.Expiry)))
            {
                account.Expiry = rq.Expiry;
            }

            if (rq.IsModified(nameof(rq.ProductId)) && account.Times == null)
            {
                var productId = rq.ProductId;

                if (productId.HasValue)
                {
                    var hasProduct = await _db.Products(orgId).Where(p => p.Id == productId.Value).AnyAsync(cancellationToken);
                    if (!hasProduct)
                    {
                        return ApplicationErrors.NoId.AsResult(nameof(rq.ProductId));
                    }
                }

                account.ProductId = productId;
            }

            if (accountChanged)
            {
                var hasAccount = await _db.FinanceAccounts.Where(a => a.Person.OrgId == orgId
                    && a.Id != account.Id
                    && a.Kind == account.Kind
                    && a.AccountNumber == account.AccountNumber).AnyAsync(cancellationToken);

                if (hasAccount)
                {
                    return ApplicationErrors.ItemExists.AsResult(nameof(rq.AccountNumber));
                }
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties();

            // Save
            var task1 = _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateFinanceAccountMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, $"{account.Bank} - {account.AccountNumber}"),
                Changes = changes
            };
            var task2 = _queueService.PushAsync(message, CrmJsonSerializerContext.Default.UpdateFinanceAccountMessage, cancellationToken);

            await Task.WhenAll(task1, task2);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read finance account data for update
        /// 读取用于更新的财务账户数据
        /// </summary>
        /// <param name="id">Finance account id</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task UpdateReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            // Permission check
            if (!await _commonService.HasPermissionAsync((short)Permissions.Finance.Edit, cancellationToken))
            {
                return;
            }

            var orgId = User.OrganizationInt;

            await _db.FinanceAccounts.AsNoTracking()
                .Where(a => a.Id == id && a.Person.OrgId == orgId)
                .Select(a => new
                {
                    a.Id,
                    a.PersonId,
                    a.Kind,
                    a.Bank,
                    a.Currency,
                    a.AccountNumber,
                    a.Description,
                    a.Swift,
                    a.Status,
                    a.Expiry
                }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }
    }
}
