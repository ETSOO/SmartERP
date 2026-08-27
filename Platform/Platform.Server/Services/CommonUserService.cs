using com.etsoo.CoreFramework.User;
using Platform.Server.Application;

namespace Platform.Server.Services
{
    /// <summary>
    /// Common user service
    /// 通用用户服务
    /// </summary>
    public abstract class CommonUserService : CommonService, ICommonUserService
    {
        /// <summary>
        /// Current user
        /// 当前用户
        /// </summary>
        protected override CurrentUser User { get; }

        protected CommonUserService(IMyApp app, MyAppConfiguration configuration, CurrentUser user, string flag, ILogger logger)
            : base(app, configuration, user, flag, logger)
        {
            User = user;
        }

        /// <summary>
        /// Is platform admin
        /// 是否为平台管理员
        /// </summary>
        /// <returns>是否为平台管理员</returns>
        protected bool IsAdmin()
        {
            return User.AppId == MyAppConstants.AdminAppId;
        }
    }
}
