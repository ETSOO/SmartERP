using com.etsoo.CoreFramework.Business;
using com.etsoo.Database.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlatformShared.Dto;

namespace PlatformShared.Database.Models.Configurations
{
    internal class CoreOrganizationAppConfiguration : IEntityTypeConfiguration<CoreOrganizationApp>
    {
        public void Configure(EntityTypeBuilder<CoreOrganizationApp> entity)
        {
            entity.HasKey(e => e.Id).HasName("core_organization_app_pkey");

            entity.ToTable("core_organization_app");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CoreAppId).HasColumnName("core_app_id");
            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
            entity.Property(e => e.AppKey)
                .HasMaxLength(128)
                .HasColumnName("app_key");
            entity.Property(e => e.AppSecret)
                .HasMaxLength(256)
                .HasColumnName("app_secret");
            entity.Property(e => e.LocalName)
                .HasMaxLength(128)
                .HasColumnName("local_name");
            entity.Property(e => e.LocalUrls)
                .HasColumnType("jsonb")
                .HasConversion(new JsonTypeConverter<AppUrl[]?>(PlatformSharedContext.Default.AppUrlArray))
                .HasColumnName("local_urls");
            entity.Property(e => e.Expiry).HasColumnName("expiry");
            entity.Property(e => e.Status)
                .HasConversion<byte>()
                .HasDefaultValue(EntityStatus.Normal)
                .HasColumnName("status");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");

            entity.HasOne(d => d.CoreApp).WithMany(p => p.CoreOrganizationApps)
                .HasForeignKey(d => d.CoreAppId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_organization_app_core_app_id_fkey");

            entity.HasOne(d => d.CoreOrganization).WithMany(p => p.Apps)
                .HasForeignKey(d => d.CoreOrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_organization_app_core_organization_id_fkey");
        }
    }
}
