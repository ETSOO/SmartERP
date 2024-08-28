using Microsoft.EntityFrameworkCore;

namespace CoreApp.Server.Database
{
    /// <summary>
    /// My database context
    /// https://learn.microsoft.com/en-us/ef/core/modeling/
    /// 1. Using the OnModelCreating method (fluent API)
    /// 2. The other way is Data annotation
    /// 我的数据库上下文
    /// </summary>
    public partial class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }
    }
}
