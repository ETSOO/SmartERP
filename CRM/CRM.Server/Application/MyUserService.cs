using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.SmartERP;

namespace CRM.Server.Application
{
    public class MyUserService : SEUserService
    {
        public MyUserService(IMyApp app, MyAppConfiguration config, CurrentUser user, string flag, ILogger<MyUserService> logger)
            : base(app, config, user, flag, logger)
        {
        }
    }
}
