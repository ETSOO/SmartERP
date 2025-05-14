using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.SmartERP;
using PlatformShared.Database;

namespace CRM.Server.Services
{
    /// <summary>
    /// Permission group service
    /// 权限组服务
    /// </summary>
    public class PermissionGroupService : SEUserService
    {
        readonly MyDbContext _db;
        readonly ICommonService _commonService;

        public PermissionGroupService(
            MyDbContext db,
            ISEServiceApp app,
            CurrentUserAccessor userAccessor,
            ILogger<PermissionGroupService> logger,
            ICommonService commonService
        )
            : base(app, userAccessor.UserSafe, "group", logger)
        {
            _db = db;
            _commonService = commonService;
        }
    }
}