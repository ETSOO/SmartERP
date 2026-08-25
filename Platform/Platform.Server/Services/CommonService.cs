using com.etsoo.CoreFramework.Services;
using com.etsoo.CoreFramework.User;
using Platform.Server.Application;

namespace Platform.Server.Services
{
    /// <summary>
    /// Common service
    /// 通用服务
    /// </summary>
    public abstract class CommonService : ServiceBase<IMyApp, CurrentUser>, ICommonService
    {
        protected CommonService(IMyApp app, CurrentUser? user, string flag, ILogger logger)
            : base(app, user, flag, logger)
        {
        }
    }
}
