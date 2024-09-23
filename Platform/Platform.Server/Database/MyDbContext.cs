using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        /// Core applications
        /// 核心应用
        /// </summary>
        public DbSet<CoreApp> CoreApps { get; set; }

        /// <summary>
        /// Authorization codes
        /// 授权码
        /// </summary>
        public DbSet<CoreAuthCode> CoreAuthCodes { get; set; }

        /// <summary>
        /// Core organizations
        /// 核心机构
        /// </summary>
        public DbSet<CoreOrganization> CoreOrganizations { get; set; }

        /// <summary>
        /// Core organization applications
        /// 核心机构应用
        /// </summary>
        public DbSet<CoreOrganizationApp> CoreOrganizationApps { get; set; }

        /// <summary>
        /// Core organization application keys
        /// 核心机构应用密钥
        /// </summary>
        public DbSet<CoreOrganizationAppKey> CoreOrganizationAppKeys { get; set; }

        /// <summary>
        /// Core organization channels
        /// 核心机构渠道
        /// </summary>
        public DbSet<CoreOrganizationChannel> CoreOrganizationChannels { get; set; }

        /// <summary>
        /// Core organization users
        /// 核心机构用户
        /// </summary>
        public DbSet<CoreOrganizationUser> CoreOrganizationUsers { get; set; }

        /// <summary>
        /// Core users
        /// 核心用户
        /// </summary>
        public DbSet<CoreUser> CoreUsers { get; set; }

        /// <summary>
        /// Core user devices
        /// 核心用户设备
        /// </summary>
        public DbSet<CoreUserDevice> CoreUserDevices { get; set; }

        /// <summary>
        /// Core user device tokens
        /// 核心用户设备令牌
        /// </summary>
        public DbSet<CoreUserDeviceToken> CoreUserDeviceTokens { get; set; }

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
            modelBuilder.Entity<CoreApp>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_app_pkey");

                entity.ToTable("core_app");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.AppSecret)
                    .HasMaxLength(256)
                    .HasColumnName("app_secret");
                entity.Property(e => e.Creation)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("creation");
                entity.Property(e => e.Enabled).HasColumnName("enabled");
                entity.Property(e => e.WebUrl)
                    .HasMaxLength(256)
                    .HasColumnName("web_url");
                entity.Property(e => e.ApiUrl)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("api_url");
                entity.Property(e => e.HelpUrl)
                    .HasMaxLength(256)
                    .HasColumnName("help_url");
                entity.Property(e => e.IdentityType).HasColumnName("identity_type");
                entity.Property(e => e.IsPublic).HasColumnName("is_public");
                entity.Property(e => e.Logo)
                    .HasMaxLength(256)
                    .HasColumnName("logo");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnName("name");
                entity.Property(e => e.RequireLocalUrl).HasColumnName("require_local_url");
            });

            modelBuilder.Entity<CoreAuthCode>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_auth_code_pkey");

                entity.ToTable("core_auth_code");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
                entity.Property(e => e.Action).HasColumnName("action");
                entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
                entity.Property(e => e.Expiry).HasColumnName("expiry");
                entity.Property(e => e.Ip)
                    .IsRequired()
                    .HasMaxLength(45)
                    .HasConversion<IPAddressToStringConverter>()
                    .HasColumnName("ip");
                entity.Property(e => e.Openid)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("openid");
                entity.Property(e => e.Times)
                    .HasDefaultValue((short)0)
                    .HasColumnName("times");

                entity.HasOne(d => d.CoreUser).WithMany(p => p.CoreUserAuthCodes)
                    .HasForeignKey(d => d.CoreUserId)
                    .HasConstraintName("core_auth_code_core_user_id_fkey");
            });

            modelBuilder.Entity<CoreOrganization>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_organization_pkey");

                entity.ToTable("core_organization");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(1001L)
                    .HasColumnName("id");
                entity.Property(e => e.Brand)
                    .HasMaxLength(30)
                    .HasColumnName("brand");
                entity.Property(e => e.Creation)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("creation");
                entity.Property(e => e.Logo)
                    .HasMaxLength(256)
                    .HasColumnName("logo");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnName("name");
                entity.Property(e => e.OwnerId).HasColumnName("owner_id");
                entity.Property(e => e.ParentId).HasColumnName("parent_id");
                entity.Property(e => e.Pin)
                    .HasMaxLength(20)
                    .HasColumnName("pin");
                entity.Property(e => e.Status)
                    .HasConversion<byte>()
                    .HasDefaultValue(EntityStatus.Normal)
                    .HasColumnName("status");
                entity.Property(e => e.Uid).HasColumnName("uid");

                entity.HasOne(d => d.Owner).WithMany(p => p.CoreOrganizations)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_organization_owner_id_fkey");

                entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("core_organization_parent_id_fkey");
            });

            modelBuilder.Entity<CoreOrganizationApp>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_organization_app_pkey");

                entity.ToTable("core_organization_app");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasColumnName("id");
                entity.Property(e => e.CoreAppId).HasColumnName("core_app_id");
                entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
                entity.Property(e => e.Creation)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("creation");
                entity.Property(e => e.Expiry).HasColumnName("expiry");
                entity.Property(e => e.Status)
                    .HasConversion<byte>()
                    .HasDefaultValue(EntityStatus.Normal)
                    .HasColumnName("status");

                entity.HasOne(d => d.CoreApp).WithMany(p => p.CoreOrganizationApps)
                    .HasForeignKey(d => d.CoreAppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_organization_app_core_app_id_fkey");

                entity.HasOne(d => d.CoreOrganization).WithMany(p => p.CoreOrganizationApps)
                    .HasForeignKey(d => d.CoreOrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_organization_app_core_organization_id_fkey");
            });

            modelBuilder.Entity<CoreOrganizationAppKey>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_organization_app_key_pkey");

                entity.ToTable("core_organization_app_key");

                entity.HasIndex(e => new { e.CoreOrganizationAppId, e.AppKey }, "core_organization_app_key_core_organization_app_id_app_key_idx")
                    .IsUnique()
                    .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasColumnName("id");
                entity.Property(e => e.CoreOrganizationAppId).HasColumnName("core_organization_app_id");
                entity.Property(e => e.AppKey)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnName("app_key");
                entity.Property(e => e.AppSecret)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("app_secret");
                entity.Property(e => e.LocalApi)
                    .HasMaxLength(256)
                    .HasColumnName("local_api");
                entity.Property(e => e.LocalName)
                    .HasMaxLength(128)
                    .HasColumnName("local_name");
                entity.Property(e => e.LocalUrl)
                    .HasMaxLength(256)
                    .HasColumnName("local_url");
                entity.Property(e => e.Creation)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("creation");

                entity.HasOne(d => d.CoreOrganizationApp).WithMany(p => p.CoreOrganizationAppKeys)
                    .HasForeignKey(d => d.CoreOrganizationAppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_organization_app_key_core_organization_app_id_fkey");
            });

            modelBuilder.Entity<CoreOrganizationChannel>(entity =>
            {
                entity.HasKey(e => new { e.PartnerId, e.OwnerId }).HasName("core_organization_channel_pkey");

                entity.ToTable("core_organization_channel");

                entity.Property(e => e.PartnerId).HasColumnName("partner_id");
                entity.Property(e => e.OwnerId).HasColumnName("owner_id");
                entity.Property(e => e.Creation)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("creation");
                entity.Property(e => e.Enabled).HasColumnName("enabled");
                entity.Property(e => e.RefreshTime)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("refresh_time");

                entity.HasOne(d => d.Owner).WithMany(p => p.CoreOrganizationChannelOwners)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_organization_channel_owner_id_fkey");

                entity.HasOne(d => d.Partner).WithMany(p => p.CoreOrganizationChannelPartners)
                    .HasForeignKey(d => d.PartnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_organization_channel_partner_id_fkey");
            });

            modelBuilder.Entity<CoreOrganizationUser>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_organization_user_pkey");

                entity.ToTable("core_organization_user");

                entity.HasIndex(e => new { e.CoreOrganizationId, e.CoreUserId }, "core_organization_user_core_organization_id_core_user_id_idx")
                    .IsUnique()
                    .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "false");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(1001L)
                    .HasColumnName("id");
                entity.Property(e => e.AssignedId)
                    .HasMaxLength(20)
                    .HasColumnName("assigned_id");
                entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
                entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
                entity.Property(e => e.Creation)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("creation");
                entity.Property(e => e.Data)
                    .HasColumnType("jsonb")
                    .HasColumnName("data");
                entity.Property(e => e.Expiry).HasColumnName("expiry");
                entity.Property(e => e.IdentityType)
                    .HasConversion<byte>()
                    .HasColumnName("identity_type");
                entity.Property(e => e.LocalAvatar)
                    .HasMaxLength(256)
                    .HasColumnName("local_avatar");
                entity.Property(e => e.LocalName)
                    .HasMaxLength(128)
                    .HasColumnName("local_name");
                entity.Property(e => e.Permission).HasColumnName("permission");
                entity.Property(e => e.Pinyin)
                    .HasMaxLength(20)
                    .HasColumnName("pinyin");
                entity.Property(e => e.RefreshTime)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("refresh_time");
                entity.Property(e => e.Status)
                    .HasConversion<byte>()
                    .HasDefaultValue(EntityStatus.Normal)
                    .HasColumnName("status");
                entity.Property(e => e.Uid)
                    .HasDefaultValueSql("gen_random_uuid()")
                    .HasColumnName("uid");
                entity.Property(e => e.UserRole)
                    .HasConversion<short>()
                    .HasColumnName("user_role");

                entity.HasOne(d => d.CoreOrganization).WithMany(p => p.CoreOrganizationUsers)
                    .HasForeignKey(d => d.CoreOrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_organization_user_core_organization_id_fkey");

                entity.HasOne(d => d.CoreUser).WithMany(p => p.CoreOrganizationUsers)
                    .HasForeignKey(d => d.CoreUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_organization_user_core_user_id_fkey");
            });

            modelBuilder.Entity<CoreUser>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_user_pkey");

                entity.ToTable("core_user");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(1001L)
                    .HasColumnName("id");
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
                entity.Property(e => e.Region)
                    .HasMaxLength(2)
                    .HasColumnName("region");
                entity.Property(e => e.Pin)
                    .HasMaxLength(20)
                    .HasColumnName("pin");
                entity.Property(e => e.LatestOrganizationId)
                    .HasColumnName("latest_organization_id");
            });

            modelBuilder.Entity<CoreUserDevice>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_user_device_pkey");

                entity.ToTable("core_user_device");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(1001L)
                    .HasColumnName("id");
                entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
                entity.Property(e => e.Creation)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("creation");
                entity.Property(e => e.DeviceType)
                    .HasConversion<byte>()
                    .HasColumnName("device_type");
                entity.Property(e => e.LastLogin)
                    .HasDefaultValueSql("now()")
                    .HasColumnName("last_login");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnName("name");
                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("client_id");

                entity.HasOne(d => d.CoreUser).WithMany(p => p.CoreUserDevices)
                    .HasForeignKey(d => d.CoreUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_user_device_core_user_id_fkey");
            });

            modelBuilder.Entity<CoreUserDeviceToken>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_user_device_token_pkey");

                entity.ToTable("core_user_device_token");

                entity.HasIndex(e => new { e.DeviceId, e.AppId, e.Token }, "core_user_device_token_device_id_app_id_token_idx")
                    .IsUnique()
                    .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasColumnName("id");
                entity.Property(e => e.ResponseType)
                    .HasConversion<byte>()
                    .HasColumnName("response_type");
                entity.Property(e => e.AppId).HasColumnName("app_id");
                entity.Property(e => e.AppKeyId).HasColumnName("app_key_id");
                entity.Property(e => e.Culture)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("culture");
                entity.Property(e => e.Data)
                    .IsRequired()
                    .HasColumnType("jsonb")
                    .HasColumnName("data");
                entity.Property(e => e.DeviceId).HasColumnName("device_id");
                entity.Property(e => e.Expiry).HasColumnName("expiry");
                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("token");

                entity.HasOne(d => d.App).WithMany(p => p.CoreUserDeviceTokens)
                    .HasForeignKey(d => d.AppId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_user_device_token_app_id_fkey");

                entity.HasOne(d => d.Device).WithMany(p => p.CoreUserDeviceTokens)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("core_user_device_token_device_id_fkey");
            });

            modelBuilder.Entity<CoreUserIdentifier>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("core_user_identifier_pkey");

                entity.ToTable("core_user_identifier");

                entity.HasIndex(e => new { e.Type, e.Value }, "core_user_identifier_type_value_id_core_user_id_ref_type_idx")
                    .IsUnique()
                    .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "false");

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
