using com.etsoo.Core.Database;
using com.etsoo.Core.Storage;
using com.etsoo.Core.Utils;

namespace com.etsoo.Core.Application
{
    /// <summary>
    /// Commom Application, singleton supported
    /// 通用程序，支持单例
    /// </summary>
    public abstract class ApplicationBase : IApplication
    {
        /// <summary>
        /// Configuration
        /// 配置
        /// </summary>
        public ICoreConfiguration Configuration { get; }

        /// <summary>
        /// Database
        /// 数据库
        /// </summary>
        public ICommonDatabase Database { get; }

        /// <summary>
        /// Storage
        /// 存储
        /// </summary>
        public IStorage Storage { get; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <param name="database">Database</param>
        /// <param name="storage">Storage</param>
        public ApplicationBase(ICoreConfiguration configuration, ICommonDatabase database, IStorage storage)
        {
            Configuration = configuration;
            Database = database;
            Storage = storage;
        }

        /// <summary>
        /// Hash password
        /// 哈希密码
        /// </summary>
        /// <param name="password">Raw password</param>
        /// <returns>Hashed password</returns>
        public string HashPassword(string password)
        {
            return CryptographyUtil.HMACSHA512(password, Configuration.PrivateKey + Configuration.PrivateDeploymentId);
        }
    }
}