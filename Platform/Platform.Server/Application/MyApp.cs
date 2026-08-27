using com.etsoo.CoreFramework.Application;
using com.etsoo.Database;
using Npgsql;

namespace Platform.Server.Application
{
    /// <summary>
    /// Main application
    /// 主程序
    /// </summary>
    public class MyApp : CoreApplication<NpgsqlConnection>, IMyApp
    {
        public MyApp(IServiceCollection services, string privateKey, IDatabase<NpgsqlConnection> db, bool modelValidated = false)
            : base(db, privateKey, modelValidated)
        {
        }
    }
}