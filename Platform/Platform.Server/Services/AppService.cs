using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.User;
using com.etsoo.Database;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Crypto;
using com.etsoo.Utils.Serialization;
using com.etsoo.Utils.String;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using Platform.Server.Dto.App;
using Platform.Server.Endpoints.App.RQ;
using Platform.Server.Endpoints.Org.RQ;
using PlatformShared;
using PlatformShared.Database;
using PlatformShared.Database.Models;
using PlatformShared.Extentions;
using PlatformShared.Messages;
using System.Buffers;
using System.Text.Json;

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
        readonly IQueueService _queueService;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="orgService">Organization service</param>
        /// <param name="queueService">Queue service</param>
        public AppService(MyDbContext db, IMyApp app, CurrentUserAccessor userAccessor, ILogger<AppService> logger,
            IOrgService orgService,
            IQueueService queueService)
            : base(app, userAccessor.UserSafe, "app", logger)
        {
            _db = db;
            _orgService = orgService;
            _queueService = queueService;
        }

        /// <summary>
        /// Buy application
        /// 购买应用
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <returns>Result</returns>
        public Task<IActionResult> BuyAsync(AppBuyRQ rq, CancellationToken cancellationToken = default)
        {
            return BuyAsync(rq, false, cancellationToken);
        }

        async Task<IActionResult> BuyAsync(AppBuyRQ rq, bool newOrg, CancellationToken cancellationToken)
        {
            // Check the application
            var app = await _db.CoreApps.AsNoTracking()
                .Where(a => a.Id == rq.Id && a.Enabled && a.IsPublic)
                .Select(a => new { a.Name })
                .FirstOrDefaultAsync(cancellationToken);

            if (app == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Check the organization
            if (!await _orgService.OwnsAsync(rq.OrganizationId, UserRole.User, cancellationToken))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.OrganizationId));
            }

            // No duplicate purchase
            if (await _db.CoreOrganizationApps.AnyAsync(oa => oa.CoreAppId == rq.Id && oa.CoreOrganizationId == rq.OrganizationId, cancellationToken: cancellationToken))
            {
                return ApplicationErrors.ItemExists.AsResult();
            }

            // Default months
            var months = rq.Months.GetValueOrDefault(12);

            // Repository
            _db.CoreOrganizationApps.Add(new CoreOrganizationApp
            {
                CoreAppId = rq.Id,
                CoreOrganizationId = rq.OrganizationId,
                Expiry = DateTimeOffset.UtcNow.AddMonths(months)
            });

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new BuyAppMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, app.Name),
                Months = months,
                OrgId = rq.OrganizationId,
                NewOrg = newOrg
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.BuyAppMessage, cancellationToken);

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
                Pin = rq.OrgPin,
                Region = rq.Region
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

            result = await BuyAsync(buyRq, true, cancellationToken);

            if (result.Ok && id.HasValue)
            {
                result.Data[nameof(id)] = id.Value;
            }

            return result;
        }

        /// <summary>
        /// Create API key
        /// 创建API密钥
        /// </summary>
        /// <param name="id">App id</param>
        /// <param name="passphase">Passphase</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> CreateApiKeyAsync(int id, string passphase, CancellationToken cancellationToken = default)
        {
            var app = await _db.CoreOrganizationApps
                .AsNoTracking()
                .Where(oa => oa.Id == id && oa.CoreOrganizationId == User.OrganizationInt && oa.Status == EntityStatus.Normal)
                .Select(oa => new { AppId = oa.CoreAppId, Name = oa.LocalName ?? oa.CoreApp.Name })
                .FirstOrDefaultAsync(cancellationToken);

            if (app == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            var appKey = Guid.NewGuid().ToString("N");
            var appSecret = await App.HashPasswordAsync(id + CryptographyUtils.CreateRandString(RandStringKind.All, 32).ToString());
            var appSecretDB = App.EncriptData(appSecret, "Token" + app.AppId);

            await _db.CoreOrganizationApps.Where(oa => oa.Id == id)
                .ExecuteUpdateAsync(oa => oa.SetProperty(oa => oa.AppKey, appKey).SetProperty(oa => oa.AppSecret, appSecretDB), cancellationToken);

            // Push message
            var message = new CreateApiKeyMessage
            {
                Data = User.CreateMessageData(App.AppId, id, app.Name)
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.CreateApiKeyMessage, cancellationToken);

            var result = ActionResult.Success;
            result.Data[nameof(appKey)] = appKey;
            result.Data[nameof(appSecret)] = EncryptWeb(appSecret, passphase);

            return result;
        }

        /// <summary>
        /// Get user's latest accessed applications
        /// 获取用户最近访问的应用
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task<IEnumerable<AppData>> GetMyAsync(AppGetMyRQ rq, CancellationToken cancellationToken = default)
        {
            // Current user's organization
            var orgId = User.OrganizationInt;

            // Current user's states
            var typeFlag = Enum.Parse<IdentityTypeFlags>(rq.IdentityType.ToString());
            var isUserValid = await _db.Persons.AsNoTracking()
                .AnyAsync(ou => ou.CoreOrganizationId == orgId
                    && ou.CoreUserId == User.IdInt
                    && (ou.IdentityType.HasValue && ou.IdentityType.Value.HasFlag(typeFlag))
                    && ou.Status <= EntityStatus.Approved
                    && (ou.Expiry == null || ou.Expiry >= DateTimeOffset.UtcNow), cancellationToken);

            List<AppData> apps = [];

            List<int> defaultApps = [1];

            // Max items except the core app
            var maxItems = rq.MaxItems - 1;

            if (User.Scopes?.Contains(MyAppConstants.AdminApp) is true)
            {
                // Admin app
                defaultApps.Add(MyAppConstants.AdminAppId);
                maxItems--;
            }

            if (isUserValid)
            {
                // Current user's latest applications
                var ids = await _db.CoreUsers.AsNoTracking()
                    .Where(u => u.Id == User.IdInt)
                    .Select(u => u.LatestAppIds).FirstOrDefaultAsync(cancellationToken) ?? [];

                // Current user's organization's latest applications
                var query = _db.CoreOrganizationApps
                     .AsNoTracking()
                     .Where(oa => oa.CoreOrganizationId == orgId
                         && oa.Status <= EntityStatus.Approved
                         && (oa.Expiry == null || oa.Expiry >= DateTimeOffset.UtcNow)
                         && oa.CoreApp.IdentityType == rq.IdentityType)
                     .Select(oa => new AppData
                     {
                         Id = oa.CoreAppId,
                         Name = oa.CoreApp.Name,
                         LocalName = oa.LocalName,
                         Urls = oa.LocalUrls ?? oa.CoreApp.Urls,
                         Logo = oa.CoreApp.Logo
                     });

                if (ids.Count > 0)
                {
                    apps.AddRange(await query.Where(ou => ids.Contains(ou.Id)).Take(maxItems).ToListAsync(cancellationToken));
                    apps = [.. apps.OrderBy(ou => ids.IndexOf(ou.Id))];
                }

                var left = maxItems - apps.Count;
                if (left > 0)
                {
                    if (ids.Count > 0)
                    {
                        apps.AddRange(await query.Where(ou => !ids.Contains(ou.Id)).OrderByDescending(ou => ou.Id).Take(left).ToListAsync(cancellationToken));
                    }
                    else
                    {
                        apps.AddRange(await query.OrderByDescending(ou => ou.Id).Take(left).ToListAsync(cancellationToken));
                    }
                }
            }

            // Add the default apps
            apps.AddRange(await _db.CoreApps.Where(a => defaultApps.Contains(a.Id)).Select(a => new AppData
            {
                Id = a.Id,
                Name = a.Name,
                Urls = a.Urls,
                Logo = a.Logo
            }).ToListAsync(cancellationToken));

            return apps;
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
            var apps = await GetMyAsync(rq, cancellationToken);
            await writer.SerializeAsync(apps, MyJsonSerializerContext.Default.IEnumerableAppData);

            /*
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
            */
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
                Urls = a.Urls,
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
            var (hasContent, commandText) =  await _db.CoreOrganizationApps
                .AsNoTracking()
                .Where(oa => oa.CoreOrganizationId == User.OrganizationInt)
                .QueryEtsoo(rq, oa => oa.Id, oa => oa.Status, (q) =>
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

                        q = q.QueryEtsooKeywords(keyword, DbUtils.ILikeMethod, oa => oa.LocalName ?? oa.CoreApp.Name);
                    }

                    if (rq.Expiry.HasValue)
                    {
                        q = q.Where(oa => oa.Expiry < rq.Expiry);
                    }

                    if (rq.ExpiryDays.HasValue)
                    {
                        var expiryDays = rq.ExpiryDays.Value;
                        q = q.Where(oa => oa.Expiry < DateTimeOffset.UtcNow.AddDays(expiryDays));
                    }

                    return q;
                }).Select(oa => new AppPurchasedQueryData
                {
                    Id = oa.Id,
                    Name = oa.CoreApp.Name,
                    LocalName = oa.LocalName,
                    IdentityType = oa.CoreApp.IdentityType,
                    RequireLocalUrl = oa.CoreApp.RequireLocalUrl,
                    Urls = oa.LocalUrls ?? oa.CoreApp.Urls,
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

        /// <summary>
        /// Read app data for view
        /// 读取用于浏览的应用数据
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task ReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = _db.CoreOrganizationApps
                .AsNoTracking()
                .Where(oa => oa.Id == id && oa.CoreOrganizationId == User.OrganizationInt);

            await query.Select(oa => new
            {
                oa.Id,
                oa.AppKey,
                oa.LocalName,
                oa.LocalUrls,
                oa.Expiry,
                ExpiryDays = oa.Expiry == null || oa.Expiry <= DateTimeOffset.UtcNow.AddDays(-90) ? (int?)null : (int)(oa.Expiry.Value - DateTimeOffset.UtcNow).TotalDays,
                oa.Status,
                oa.Creation,

                oa.CoreApp.IdentityType,
                AppId = oa.CoreAppId,
                oa.CoreApp.Name,
                oa.CoreApp.Urls
            }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
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
            var app = await _db.CoreOrganizationApps.AsNoTracking()
                .Where(oa => oa.Id == rq.Id && oa.CoreOrganizationId == User.OrganizationInt)
                .Select(oa => new { Name = oa.LocalName ?? oa.CoreApp.Name })
                .FirstOrDefaultAsync(cancellationToken);

            if (app == null)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(rq.Id));
            }

            // Update the expiry
            await _db.CoreOrganizationApps.AsNoTracking()
                .Where(oa => oa.Id == rq.Id)
                .ExecuteUpdateAsync(oa => oa.SetProperty(a => a.Expiry, a => a.Expiry == null ? DateTimeOffset.UtcNow.AddMonths(rq.Months) : a.Expiry.Value.AddMonths(rq.Months)), cancellationToken);

            // Push message
            var message = new RenewAppMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, app.Name),
                Months = rq.Months
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.RenewAppMessage, cancellationToken);

            return ActionResult.Success;
        }

        /// <summary>
        /// Update
        /// 更新
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<IActionResult> UpdateAsync(AppUpdateRQ rq, CancellationToken cancellationToken = default)
        {
            var app = await _db.CoreOrganizationApps.FirstOrDefaultAsync(oa => oa.Id == rq.Id && oa.CoreOrganizationId == User.OrganizationInt, cancellationToken);
            if (app == null)
            {
                return ApplicationErrors.NoId.AsResult();
            }

            // Name
            var name = await _db.CoreOrganizationApps
                .AsNoTracking()
                .Where(oa => oa.Id == rq.Id)
                .Select(oa => oa.LocalName ?? oa.CoreApp.Name)
                .FirstOrDefaultAsync(cancellationToken);

            // Update
            if (rq.IsModified(nameof(rq.LocalName)))
            {
                app.LocalName = rq.LocalName;
            }

            if (rq.IsModified(nameof(rq.LocalUrls)))
            {
                app.LocalUrls = rq.LocalUrls;
            }

            if (rq.IsModified(nameof(rq.Status)) && rq.Status.HasValue)
            {
                app.Status = rq.Status.Value;
            }

            // Changes
            var changes = _db.ChangeTracker.Entries().GetChangedProperties((name, value) =>
            {
                if (name == nameof(rq.LocalUrls) && value != null)
                {
                    return JsonSerializer.Serialize(value, PlatformSharedContext.Default.AppUrlArray);
                }
                else
                {
                    return StringUtils.GetPrimitiveValue(value);
                }
            });

            // Save
            await _db.SaveChangesAsync(cancellationToken);

            // Push message
            var message = new UpdateAppMessage
            {
                Data = User.CreateMessageData(App.AppId, rq.Id, name),
                Changes = changes
            };
            await _queueService.PushAsync(message, PlatformSharedContext.Default.UpdateAppMessage, cancellationToken);

            // Return
            return ActionResult.Succeed(rq.Id);
        }

        /// <summary>
        /// Read data for update
        /// 读取用于更新的数据
        /// </summary>
        /// <param name="id">App id</param>
        /// <param name="writer">Writer to hold the data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task UpdateReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default)
        {
            var query = _db.CoreOrganizationApps
                .AsNoTracking()
                .Where(oa => oa.Id == id && oa.CoreOrganizationId == User.OrganizationInt);

            await query.Select(oa => new
            {
                oa.Id,
                oa.LocalName,
                oa.LocalUrls,
                oa.Status,
                oa.CoreApp.Name
            }).ToJsonObjectAsync(writer, cancellationToken: cancellationToken);
        }
    }
}
