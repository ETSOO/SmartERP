using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.Database;
using com.etsoo.Utils.Crypto;
using com.etsoo.Utils.String;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Collections.Concurrent;
using System.Text;

namespace Platform.Server.Application
{
    /// <summary>
    /// Main application
    /// 主程序
    /// </summary>
    public class MyApp : CoreApplication<MyAppConfiguration, NpgsqlConnection>, IMyApp
    {
        private static readonly ConcurrentDictionary<int, string> privateKeys = new();
        private static readonly ConcurrentDictionary<int, string> encriptionKeys = new();

        /// <summary>
        /// Authentication service
        /// 验证服务
        /// </summary>
        public IAuthService? AuthService { get; init; }

        public MyApp(IServiceCollection services, MyAppConfiguration configuration, IDatabase<NpgsqlConnection> db, JwtSettings? jwtSettings, JwtBearerEvents? events = null, bool modelValidated = false) : base(configuration, db, modelValidated)
        {
            if (jwtSettings != null)
            {
                AuthService = new JwtService(services, jwtSettings, (token, securityToken, kid, validationParameters) =>
                {
                    var keys = new List<SecurityKey>();
                    var (serviceId, _) = StringUtils.SplitIntGuid(kid);

                    if (serviceId != null)
                    {
                        var id = serviceId.Value;
                        if (!privateKeys.TryGetValue(id, out var key))
                        {

                        }

                        if (key != null)
                        {
                            var crypto = new RSACrypto(null, key);
                            keys.Add(new RsaSecurityKey(crypto.RSA) { KeyId = kid });
                        }
                    }

                    return keys;
                }, (token, securityToken, kid, validationParameters) =>
                {
                    var keys = new List<SecurityKey>();
                    var (serviceId, _) = StringUtils.SplitIntGuid(kid);

                    if (serviceId != null)
                    {
                        var id = serviceId.Value;
                        if (!encriptionKeys.TryGetValue(id, out var key))
                        {

                        }

                        if (key != null)
                            keys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) { KeyId = kid });
                    }

                    return keys;
                }, events);
            }
        }
    }
}