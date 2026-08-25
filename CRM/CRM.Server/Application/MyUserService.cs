using com.etsoo.CoreFramework.User;
using com.etsoo.ServiceApp.SmartERP;

namespace CRM.Server.Application
{
    public class MyUserService : SEUserService<MyAppConfiguration>
    {
        public MyUserService(IMyApp app, CurrentUser user, string flag, ILogger<MyUserService> logger)
            : base(app, user, flag, logger)
        {
        }
    }
}
