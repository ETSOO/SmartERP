using Microsoft.EntityFrameworkCore;
using Platform.Server.Database.Models;

namespace Platform.Server.Database
{
    /// <summary>
    /// My database context
    /// 1. Using the OnModelCreating method (fluent API)
    /// 2. The other way is Data annotation
    /// 我的数据库上下文
    /// </summary>
    public partial class MyDbContext : DbContext
    {
        /// <summary>
        /// Core users
        /// 核心用户
        /// </summary>
        public DbSet<CoreUser> CoreUsers { get; set; }

        /// <summary>
        /// Core user identifiers for login
        /// 核心用户登录编号
        /// </summary>
        public DbSet<CoreUserIdentifier> CoreUserIdentifiers { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }
    }
}
