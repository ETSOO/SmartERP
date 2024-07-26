using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Database.Models;

namespace Platform.Server.Database
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CoreUser>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_user_pkey");

                entity.ToTable("core_user");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(1001L)
                    .HasColumnName("id");
                entity.Property(e => e.AssignedId)
                    .HasMaxLength(20)
                    .HasColumnName("assigned_id");
                entity.Property(e => e.Avatar)
                    .HasMaxLength(256)
                    .HasColumnName("avatar");
                entity.Property(e => e.Creation)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("creation");
                entity.Property(e => e.FamilyName)
                    .HasMaxLength(50)
                    .HasColumnName("family_name");
                entity.Property(e => e.ForeignName)
                    .HasMaxLength(128)
                    .HasColumnName("foreign_name");
                entity.Property(e => e.FrozenTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("frozen_time");
                entity.Property(e => e.GivenName)
                    .HasMaxLength(50)
                    .HasColumnName("given_name");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnName("name");
                entity.Property(e => e.Password)
                    .HasMaxLength(128)
                    .HasColumnName("password");
                entity.Property(e => e.Status)
                    .HasConversion<byte>()
                    .HasDefaultValue(EntityStatus.Normal)
                    .HasColumnName("status");
                entity.Property(e => e.Step)
                    .HasColumnName("step");
            });

            modelBuilder.Entity<CoreUserIdentifier>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_user_identifier_pkey");

                entity.ToTable("core_user_identifier");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasColumnName("id");
                entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
                entity.Property(e => e.Creation)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("creation");
                entity.Property(e => e.RefType).HasColumnName("ref_type");
                entity.Property(e => e.Type).HasColumnName("type");
                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("value");

                entity.HasOne(d => d.CoreUser).WithMany(p => p.CoreUserIdentifiers)
                    .HasForeignKey(d => d.CoreUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_user_identifier_core_user_id_fkey");
            });
        }
    }
}
