using com.etsoo.Database;
using com.etsoo.ServiceApp.SmartERP;
using Npgsql;

namespace Admin.Server.Application
{
    public class MyApp : SEServiceApp<MyAppConfiguration>, IMyApp
    {
        public MyApp(IServiceCollection services, MyAppConfiguration configuration, IDatabase<NpgsqlConnection> db, bool modelValidated = false)
            : base(services, configuration, db, modelValidated, 2)
        {
        }
    }
}
