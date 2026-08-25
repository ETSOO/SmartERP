using com.etsoo.CoreFramework.Application;
using Npgsql;

namespace Platform.Server.Application
{
    public interface IMyApp : ICoreApplication<MyAppConfiguration, NpgsqlConnection>
    {
    }
}
