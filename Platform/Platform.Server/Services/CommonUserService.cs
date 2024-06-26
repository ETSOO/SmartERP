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

        protected CommonUserService(IMyApp app, CurrentUser user, string flag, ILogger logger)
            : base(app, user, flag, logger)
        {
            User = user;
        }
    }
}
