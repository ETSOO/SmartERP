using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using Platform.Server.Dto.App;
using Platform.Server.Endpoints.App.RQ;
using Platform.Server.Endpoints.Org.RQ;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using System.Buffers;

namespace Platform.Server.Services
{
    /// <summary>
    /// Application service
    /// 程序服务
    /// </summary>
    public class AppService : CommonUserService, IAppService
    {
        readonly MyDbContext _db;
        readonly IOrgService _orgService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="orgService">Organization service</param>
        public AppService(MyDbContext db, IMyApp app, CurrentUserAccessor userAccessor, ILogger<PublicService> logger,
            IOrgService orgService)
            : base(app, userAccessor.UserSafe, "app", logger)
        {
            _db = db;
            _orgService = orgService;
        }

        /// <summary>
        /// Buy application
        /// 购买应用
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> BuyAsync(AppBuyRQ rq, CancellationToken cancellationToken = default)
        {
            // Check the application
            if (rq.Id < 3)
            {
                return ApplicationErrors.NoValidData.AsResult();
            }

            // No duplicate purchase
            if (await _db.CoreOrganizationApps.AnyAsync(oa => oa.CoreAppId == rq.Id && oa.CoreOrganizationId == User.OrganizationInt, cancellationToken: cancellationToken))
            {
                return ApplicationErrors.ItemExists.AsResult();
            }

            // Check the organization
            if (!await _orgService.OwnsAsync(rq.OrganizationId, UserRole.User, cancellationToken))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.OrganizationId));
            }

            // Repository
            await _db.CoreOrganizationApps.AddAsync(new CoreOrganizationApp
            {
                CoreAppId = rq.Id,
                CoreOrganizationId = rq.OrganizationId,
                Expiry = DateTimeOffset.UtcNow.AddYears(1)
            }, cancellationToken);

            return ActionResult.Success;
        }

        /// <summary>
        /// Buy application and create new organization
        /// 购买应用并创建新机构
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> BuyNewAsync(AppBuyNewRQ rq, CancellationToken cancellationToken = default)
        {
            // Create the organization
            var orgRq = new OrgCreateRQ
            {
                Name = rq.OrgName,
                Pin = rq.OrgPin
            };

            var (result, id) = await _orgService.CreateWithIdAsync(orgRq, cancellationToken);
            if (!result.Ok || id == null)
            {
                return result;
            }

            // Buy the application
            var buyRq = new AppBuyRQ
            {
                Id = rq.Id,
                OrganizationId = id.Value
            };

            result = await BuyAsync(buyRq, cancellationToken);

            if (result.Ok && id.HasValue)
            {
                result.Data[nameof(id)] = id.Value;
            }

            return result;
        }

        /// <summary>
        /// Get user's latest accessed applications
        /// 获取用户最近访问的应用
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task GetMyAsync(AppGetMyRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var (hasContent, commandText) = await _db.CoreOrganizationApps
                .AsNoTracking()
                .Where(oa => oa.CoreOrganizationId == User.OrganizationInt
                    && oa.Status <= EntityStatus.Approved
                    && (oa.Expiry == null || oa.Expiry >= DateTimeOffset.UtcNow)
                    && oa.CoreApp.IdentityType == rq.IdentityType)
                .OrderByDescending(oa => oa.Id)
                .Take(rq.MaxItems)
                .Select(oa => new AppQueryData
                {
                    Id = oa.Id,
                    Name = oa.LocalName ?? oa.CoreApp.Name,
                    IdentityType = oa.CoreApp.IdentityType,
                    RequireLocalUrl = oa.CoreApp.RequireLocalUrl,
                    WebUrl = oa.LocalUrl ?? oa.CoreApp.WebUrl,
                    HelpUrl = oa.LocalHelpUrl ?? oa.CoreApp.HelpUrl,
                    Logo = oa.CoreApp.Logo
                }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("GetMyAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        private IQueryable<CoreApp> CreateQuery(AppListRQ rq, Func<IQueryable<CoreApp>, IQueryable<CoreApp>>? filters = null)
        {
            var query = _db.CoreApps
                .AsNoTracking()
                .Where(a => a.IsPublic && a.Enabled)
                .QueryEtsoo(rq, (a) => a.Id, null, (q) =>
                {
                    if (rq.IdentityType.HasValue)
                    {
                        q = q.Where(a => a.IdentityType == rq.IdentityType);
                    }

                    if (rq.RequireLocalUrl.HasValue)
                    {
                        q = q.Where(a => a.RequireLocalUrl == rq.RequireLocalUrl.Value);
                    }

                    if (rq.Keyword?.Length > 1)
                    {
                        var keyword = rq.Keyword;

                        q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, a => a.Name);
                    }

                    if (filters != null)
                    {
                        q = filters(q);
                    }

                    return q;
                });
            ;

            return query;
        }

        /// <summary>
        /// List applications JSON data
        /// 应用列表JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ListAsync(AppListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq);

            await query.Select(a => new
            {
                a.Id,
                a.Name
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Query application JSON data
        /// 查询应用JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryAsync(AppQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = CreateQuery(rq);

            var (hasContent, commandText) = await query.Select(a => new AppQueryData
            {
                Id = a.Id,
                Name = a.Name,
                IdentityType = a.IdentityType,
                RequireLocalUrl = a.RequireLocalUrl,
                WebUrl = a.WebUrl,
                HelpUrl = a.HelpUrl,
                Logo = a.Logo
            }).ToJsonAsync(writer, cancellationToken: cancellationToken);

            if (_db.IsSensitiveDataLoggingEnabled)
            {
                Logger.LogInformation("QueryAsync is {hasContent} with {commandText}", hasContent, commandText);
            }
        }

        /// <summary>
        /// Query purchased applications JSON data
        /// 查询已购应用JSON数据
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task QueryPurchasedAsync(AppPurchasedQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            try
            {
                var (hasContent, commandText) =  await _db.CoreOrganizationApps
                    .AsNoTracking()
                    .Where(oa => oa.CoreOrganizationId == User.OrganizationInt)
                    .QueryEtsoo(rq, oa => oa.Id, null, (q) =>
                    {
                        if (rq.IdentityType.HasValue)
                        {
                            q = q.Where(oa => oa.CoreApp.IdentityType == rq.IdentityType);
                        }

                        if (rq.RequireLocalUrl.HasValue)
                        {
                            q = q.Where(oa => oa.CoreApp.RequireLocalUrl == rq.RequireLocalUrl.Value);
                        }

                        if (rq.Keyword?.Length > 1)
                        {
                            var keyword = rq.Keyword;

                            q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, oa => oa.CoreApp.Name, oa => oa.LocalName);
                        }

                        if (rq.Expiry.HasValue)
                        {
                            q = q.Where(oa => oa.Expiry >= rq.Expiry);
                        }

                        if (rq.ExpiryDays.HasValue)
                        {
                            var expiryDays = rq.ExpiryDays.Value;
                            q = q.Where(oa => oa.Expiry >= DateTimeOffset.UtcNow.AddDays(expiryDays));
                        }

                        return q;
                    }).Select(oa => new AppPurchasedQueryData
                    {
                        Id = oa.Id,
                        Name = oa.LocalName ?? oa.CoreApp.Name,
                        IdentityType = oa.CoreApp.IdentityType,
                        RequireLocalUrl = oa.CoreApp.RequireLocalUrl,
                        WebUrl = oa.LocalUrl ?? oa.CoreApp.WebUrl,
                        HelpUrl = oa.LocalHelpUrl ?? oa.CoreApp.HelpUrl,
                        Logo = oa.CoreApp.Logo,
                        Expiry = oa.Expiry,
                        ExpiryDays = oa.Expiry == null || oa.Expiry <= DateTimeOffset.UtcNow.AddDays(-90) ? null : (int)(oa.Expiry.Value - DateTimeOffset.UtcNow).TotalDays,
                        Status = oa.Status,
                        Creation = oa.Creation
                    }).ToJsonAsync(writer, cancellationToken: cancellationToken);

                if (_db.IsSensitiveDataLoggingEnabled)
                {
                    Logger.LogInformation("GetPurchasedAppsAsync is {hasContent} with {commandText}", hasContent, commandText);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        /// <summary>
        /// Renew application
        /// 应用续费
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> RenewAsync(AppRenewRQ rq, CancellationToken cancellationToken = default)
        {
            // Validate the organization app
            if (!await _db.CoreOrganizationApps.AnyAsync(oa => oa.Id == rq.Id && oa.CoreOrganizationId == User.OrganizationInt, cancellationToken: cancellationToken))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.Id));
            }

            // Update the expiry
            await _db.CoreOrganizationApps.Where(oa => oa.Id == rq.Id)
                .ExecuteUpdateAsync(oa => oa.SetProperty(a => a.Expiry, a => a.Expiry == null ? DateTimeOffset.UtcNow.AddMonths(rq.Months) : a.Expiry.Value.AddMonths(rq.Months)), cancellationToken);

            return ActionResult.Success;
        }
    }
}
