using Microsoft.EntityFrameworkCore;

namespace com.etsoo.Core.Database
{
    /// <summary>
    /// Common Abstract EF Database Context
    /// 通用EF数据库上下文抽象类
    /// </summary>
    public abstract class CommonDbContext<M> : DbContext where M : class
    {
        /// <summary>
        /// Entities
        /// 实体集合
        /// </summary>
        public DbSet<M> Entities { get; set; }
    }
}