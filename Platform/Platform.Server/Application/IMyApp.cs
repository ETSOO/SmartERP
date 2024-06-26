using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using Npgsql;

namespace Platform.Server.Application
{
    public interface IMyApp : ICoreApplication<MyAppConfiguration, NpgsqlConnection>
    {
        /// <summary>
        /// Authentication service
        /// 验证服务
        /// </summary>
        IAuthService? AuthService { get; init; }
    }
}
