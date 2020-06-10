using com.etsoo.Core.Database;
using com.etsoo.Core.Storage;

namespace com.etsoo.Core.Application
{
    /// <summary>
    /// Application interface
    /// 程序接口
    /// </summary>
    public interface IApplication
    {
        /// <summary>
        /// Configuration
        /// 配置
        /// </summary>
        ICoreConfiguration Configuration { get; }

        /// <summary>
        /// Database
        /// 数据库
        /// </summary>
        ICommonDatabase Database { get; }

        /// <summary>
        /// Storage
        /// 存储
        /// </summary>
        IStorage Storage { get; }

        /// <summary>
        /// Hash password
        /// 哈希密码
        /// </summary>
        /// <param name="password">Raw password</param>
        /// <returns>Hashed password</returns>
        string HashPassword(string password);
    }
}