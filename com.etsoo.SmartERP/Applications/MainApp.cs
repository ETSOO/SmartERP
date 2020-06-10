using com.etsoo.Core.Application;
using com.etsoo.Core.Database;
using com.etsoo.Core.Storage;
using System;

namespace com.etsoo.SmartERP.Applications
{
    /// <summary>
    /// Main Application of SmartERP
    /// SmartERP主程序
    /// </summary>
    public class MainApp : ApplicationBase, IMainApp
    {
        // Create configuration
        private static MainConfiguration CreateConfiguration(Action<MainConfiguration.Builder> action)
        {
            // Create a builder
            var builder = new MainConfiguration.Builder();

            // Call action to collect properties
            action(builder);

            // Build and return
            return builder.Build();
        }

        /// <summary>
        /// Configuration
        /// 配置
        /// </summary>
        public new MainConfiguration Configuration { get; }

        /// <summary>
        /// Database
        /// 数据库
        /// </summary>
        public new SqlServerDatabase Database { get; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <param name="database">Database</param>
        public MainApp(MainConfiguration configuration, SqlServerDatabase database) : base(configuration, database, new LocalStorage())
        {
            // Setup data parameter parser
            database.DataParameterParser = new DataParameterParser();

            Configuration = configuration;
            Database = database;
        }

        /// <summary>
        /// Constructor with configuration delegate
        /// 配置代理的构造函数，简化调用
        /// </summary>
        /// <param name="configurationAction">Configuration delegate</param>
        /// <param name="database">Database</param>
        public MainApp(Action<MainConfiguration.Builder> configurationAction, SqlServerDatabase database) : this(CreateConfiguration(configurationAction), database)
        {
        }
    }
}