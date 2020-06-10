using com.etsoo.Core.Application;
using com.etsoo.Core.Database;

namespace com.etsoo.SmartERP.Applications
{
    /// <summary>
    /// Main app interface
    /// 主程序接口
    /// </summary>
    public interface IMainApp : IApplication
    {
        /// <summary>
        /// Configuration
        /// 配置
        /// </summary>
        new MainConfiguration Configuration { get; }

        /// <summary>
        /// Database
        /// 数据库
        /// </summary>
        new SqlServerDatabase Database { get; }
    }
}