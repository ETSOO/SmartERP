using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Npgsql;

namespace Platform.Server.Application
{
    /// <summary>
    /// Main application
    /// 主程序
    /// </summary>
    public class MyApp : CoreApplication<MyAppConfiguration, NpgsqlConnection>, IMyApp
    {
        /// <summary>
        /// Authentication service
        /// 验证服务
        /// </summary>
        public IAuthService? AuthService { get; init; }

        public MyApp(IServiceCollection services, MyAppConfiguration configuration, IDatabase<NpgsqlConnection> db, JwtSettings? jwtSettings, JwtBearerEvents? events = null, bool modelValidated = false) : base(configuration, db, modelValidated)
        {
            if (jwtSettings != null)
            {
                AuthService = new JwtService(services, jwtSettings, events);
            }
        }
    }
}