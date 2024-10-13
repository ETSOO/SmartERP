using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Platform.Server.Application;
using Platform.Server.Database;
using Platform.Server.Dto.App;

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
        public AppService(MyDbContext db, IMyApp app, IMyUserAccessor userAccessor, ILogger<PublicService> logger, IDistributedCache cache, IHttpContextAccessor accessor)
            : base(app, userAccessor.UserSafe, "app", logger)
        {
            _db = db;
            _cache = cache;
            _accessor = accessor;
        }

        /// <summary>
        /// Get user appliations depends on token, relogin is required for update
        /// 基于令牌获取用户程序，更新需要重新登录
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Result</returns>
        public async Task<IEnumerable<AppData>> GetAppsAsync(CancellationToken cancellationToken = default)
        {
            // User apps
            var ids = new List<int>
            {
                1 // Core app id
            };

            if (User.Scopes != null)
            {
                // Super user
                if (User.Scopes.Contains("super")) ids.Add(2);

                // Other apps
                foreach (var scope in User.Scopes)
                {
                    if (scope.StartsWith("app") && int.TryParse(scope[3..], out var id) && id > 0) ids.Add(id);
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

            return apps.UnionBy(userApps, a => a.Id);
        }
    }
}
