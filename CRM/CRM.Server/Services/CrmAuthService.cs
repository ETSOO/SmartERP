using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.SmartERP;
using com.etsoo.Utils.Actions;
using CRM.Server.Application;
using Microsoft.EntityFrameworkCore;
using PlatformShared.Database;
using System.Text.Json.Serialization.Metadata;

namespace CRM.Server.Services
{
    /// <summary>
    /// CRM authentication service
    /// CRM 认证服务
    /// </summary>
    public class CrmAuthService : SEAuthService
    {
        protected override JsonTypeInfo<ActionResult> ActionResultTypeInfo => MyJsonSerializerContext.Default.ActionResult;

        readonly MyDbContext _db;

        public CrmAuthService(IMyApp app,
            MyAppConfiguration config,
            CurrentUserAccessor userAccessor,
            ILogger<CrmAuthService> logger,
            IHttpClientFactory clientFactory,
            IAuthService authService,
            MyDbContext db
        )
            : base(app, config, userAccessor, logger, clientFactory, authService)
        {
            _db = db;
        }

        protected override async Task EnrichUserResultAsync(IActionResult result, ICurrentUser user, CancellationToken cancellationToken)
        {
            await base.EnrichUserResultAsync(result, user, cancellationToken);

            var system = await SystemService.ReadSystemSettingsAsync(_db, user.OrganizationInt, cancellationToken);

            var userPersonId = user.Oid;

            var permissionItems = await _db.PersonPermissionItems.AsNoTracking()
                .Where(p => p.PersonId == userPersonId)
                .Select(p => p.PermissionItemId)
                .ToArrayAsync(cancellationToken);

            // Add serialized data, as the result will be deserialized in the base class where lack of SystemSettings TypeInfo will cause an error
            // 添加序列化数据，因为结果将在基类中被反序列化，缺少 SystemSettings 的 TypeInfo 会导致错误

            result.Data[nameof(system)] = system;
            result.Data[nameof(userPersonId)] = userPersonId;
            result.Data[nameof(permissionItems)] = permissionItems;
        }
    }
}
