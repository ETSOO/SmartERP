using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.SmartERP;
using PlatformShared.Database;

namespace CoreApp.Server.Services
{
    /// <summary>
    /// User service
    /// 用户服务
    /// </summary>
    public class UserService : SEUserService
    {
        readonly MyDbContext _db;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        public UserService(MyDbContext db, ISEServiceApp app, CurrentUserAccessor userAccessor, ILogger<UserService> logger)
            : base(app, userAccessor.UserSafe, "user", logger)
        {
            _db = db;
        }
    }
}
