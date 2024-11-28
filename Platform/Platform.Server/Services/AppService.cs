using com.etsoo.CoreFramework.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Platform.Server.Application;
using Platform.Server.Dto.App;
using PlatformShared.Database;

namespace Platform.Server.Services
{
    /// <summary>
    /// Application service
    /// 程序服务
    /// </summary>
    public class AppService : CommonUserService, IAppService
    {
        readonly MyDbContext _db;
        readonly IDistributedCache _cache;
        readonly IHttpContextAccessor _accessor;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="cache">Cache</param>
        /// <param name="accessor">HttpContext accessor</param>
        public AppService(MyDbContext db, IMyApp app, CurrentUserAccessor userAccessor, ILogger<PublicService> logger, IDistributedCache cache, IHttpContextAccessor accessor)
            : base(app, userAccessor.UserSafe, "app", logger)
        {
            _db = db;
            _cache = cache;
            _accessor = accessor;
        }

        /// <summary>
        /// Get user's latest accessed appliation's Web URL
        /// 获取用户最近访问的程序的Web网址
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Web URL</returns>
        public async Task<string> GetUserLatestAppAsync(CancellationToken cancellationToken = default)
        {
            // Latest accessed app id
            var appId = User.AppId ?? MyAppConstants.CoreAppId;

            var url = await _db.CoreApps.AsNoTracking()
                .GroupJoin(_db.CoreOrganizationApps, a => a.Id, oa => oa.CoreAppId, (a, oa) => new { a, oa })
                .SelectMany(t => t.oa.Where(oa => oa.CoreOrganizationId == User.OrganizationInt).DefaultIfEmpty(), (t, oa) => oa == null ? t.a.WebUrl : oa.LocalUrl ?? t.a.WebUrl)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(url))
            {
                // Extreme case, get the core app url
                url = await _db.CoreApps.AsNoTracking()
                    .Where(a => a.Id == MyAppConstants.CoreAppId)
                    .Select(a => a.WebUrl)
                    .FirstAsync(cancellationToken);
            }

            return url;
        }

        /// <summary>
        /// Get user appliations depends on token, relogin is required for update
        /// 基于令牌获取用户程序，更新需要重新登录
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<AppData>> GetUserAppsAsync(CancellationToken cancellationToken = default)
        {
            // User apps
            var ids = new List<int>
            {
                MyAppConstants.CoreAppId
            };

            if (User.Scopes != null)
            {
                // Super user
                if (User.Scopes.Contains(MyAppConstants.SuperApp)) ids.Add(MyAppConstants.SuperAppId);

                // Other apps
                foreach (var scope in User.Scopes)
                {
                    ids.Add(CurrentUser.ScopeToAppId(scope));
                }
            }

            // Private apps
            var apps = await _db.CoreApps.AsNoTracking()
               .Where(a => ids.Contains(a.Id))
               .Select(a => new AppData
               {
                   Id = a.Id,
                   Name = a.Name,
                   WebUrl = a.WebUrl,
                   HelpUrl = a.HelpUrl,
                   Logo = a.Logo
               })
               .ToArrayAsync(cancellationToken);

            // User apps
            /*
            var userApps = await _db.CoreOrganizationAppKeys.AsNoTracking()
               .Where(k => ids.Contains(k.CoreOrganizationApp.CoreAppId))
               .Select(k => new AppData
               {
                   Id = k.CoreOrganizationApp.CoreAppId,
                   Name = k.LocalName ?? k.CoreOrganizationApp.CoreApp.Name,
                   WebUrl = k.LocalUrl ?? k.CoreOrganizationApp.CoreApp.WebUrl,
                   HelpUrl = k.CoreOrganizationApp.CoreApp.HelpUrl,
                   Logo = k.CoreOrganizationApp.CoreApp.Logo
               })
               .ToArrayAsync(cancellationToken);
            */

            return apps;
            //return apps.UnionBy(userApps, a => a.WebUrl);
        }
    }
}
