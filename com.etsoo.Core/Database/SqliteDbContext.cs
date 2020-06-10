using Microsoft.EntityFrameworkCore;

namespace com.etsoo.Core.Database
{
    /// <summary>
    /// Sqlite EF Database Context
    /// Sqlite EF 数据库上下文
    /// </summary>
    public class SqliteDbContext<M> : CommonDbContext<M> where M : class
    {
        private string connectionString;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public SqliteDbContext(string connectionString)
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
            optionsBuilder.UseSqlite(this.connectionString);
        }
    }
}
