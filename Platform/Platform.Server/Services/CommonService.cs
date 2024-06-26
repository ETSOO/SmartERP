using com.etsoo.CoreFramework.Services;
using com.etsoo.CoreFramework.User;
using Npgsql;
using Platform.Server.Application;

namespace Platform.Server.Services
{
    /// <summary>
    /// Common service
    /// 通用服务
    /// </summary>
    public abstract class CommonService : ServiceBase<MyAppConfiguration, NpgsqlConnection, IMyApp, CurrentUser>, ICommonService
    {
        protected CommonService(IMyApp app, CurrentUser? user, string flag, ILogger logger)
            : base(app, user, flag, logger)
        {
        }
    }
}
