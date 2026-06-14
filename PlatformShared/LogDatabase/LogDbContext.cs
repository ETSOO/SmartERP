using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlatformShared.LogDatabase.Models;

namespace PlatformShared.Database
{
    /// <summary>
    /// Log database context
    /// 日志数据库上下文
    /// </summary>
    public partial class LogDbContext : DbContext
    {
        /// <summary>
        /// Core logs
        /// 核心日志
        /// </summary>
        public required DbSet<CoreLog> CoreLogs { get; set; }

        /// <summary>
        /// Core log usages
        /// 核心日志使用量
        /// </summary>
        public required DbSet<CoreLogUsage> CoreLogUsages { get; set; }

        /// <summary>
        /// Is sensitive data logging enabled
        /// 敏感数据日志是否启用
        /// </summary>
        public readonly bool IsSensitiveDataLoggingEnabled;

        public LogDbContext(DbContextOptions<LogDbContext> options)
            : base(options)
        {
            IsSensitiveDataLoggingEnabled =  options.GetExtension<CoreOptionsExtension>().IsSensitiveDataLoggingEnabled;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CoreLog>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_log_pkey");

                entity.ToTable("core_log");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasColumnName("id");
                entity.Property(e => e.Kind)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("kind");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("title");
                entity.Property(e => e.Ip)
                    .IsRequired()
                    .HasMaxLength(45)
                    .HasConversion<IPAddressToStringConverter>()
                    .HasColumnName("ip");
                entity.Property(e => e.DeviceId).HasColumnName("device_id");
                entity.Property(e => e.Culture)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("culture");
                entity.Property(e => e.Data)
                    .HasColumnType("jsonb")
                    .HasColumnName("data");
                entity.Property(e => e.TargetId)
                    .HasColumnName("target_id");
                entity.Property(e => e.AppId)
                    .IsRequired()
                    .HasColumnName("app_id");
                entity.Property(e => e.Creation)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("creation");
            });

            modelBuilder.Entity<CoreLogUsage>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_log_usage_pkey");

                entity.ToTable("core_log_usage");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasColumnName("id");
                entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
                entity.Property(e => e.Period).HasColumnName("period");
                entity.Property(e => e.Qty).HasColumnName("qty");
            });
        }
    }
}
