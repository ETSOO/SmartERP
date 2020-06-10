using Microsoft.EntityFrameworkCore;

namespace com.etsoo.Core.Database
{
    /// <summary>
    /// SQL Server EF Database Context
    /// SQL Server EF 数据库上下文
    /// </summary>
    public class SqlServerDbContext<M> : CommonDbContext<M> where M : class
    {
        private string connectionString;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public SqlServerDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Override OnConfiguring to setup
        /// 重写配置初始化
        /// </summary>
        /// <param name="optionsBuilder">Options builder</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(this.connectionString);
        }
    }
}
