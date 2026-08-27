using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using Npgsql;

namespace CRM.Server.Application
{
    public class MyApp : SEServiceApp, IMyApp
    {
        public MyApp(IServiceCollection services, IDatabase<NpgsqlConnection> db, MyAppConfiguration configuration, bool modelValidated = false)
            : base(services, db, configuration, modelValidated, 3)
        {
        }
    }
}
