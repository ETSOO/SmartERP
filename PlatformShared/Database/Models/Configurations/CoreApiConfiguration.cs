using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class CoreApiConfiguration : IEntityTypeConfiguration<CoreApi>
    {
        public void Configure(EntityTypeBuilder<CoreApi> entity)
        {
            entity.HasKey(e => e.Id).HasName("core_api_pkey");

            entity.ToTable("core_api");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
            entity.Property(e => e.Service)
                .HasConversion<short>()
                .HasColumnName("service");
            entity.Property(e => e.Title)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName("title");
            entity.Property(e => e.Endpoint)
                .HasMaxLength(256)
                .HasColumnName("endpoint");
            entity.Property(e => e.AppId)
                .HasMaxLength(64)
                .IsRequired()
                .HasColumnName("app_id");
            entity.Property(e => e.AppSecret)
                .HasMaxLength(512)
                .IsRequired()
                .HasColumnName("app_secret");
            entity.Property(e => e.Options)
                .HasColumnType("jsonb")
                .HasColumnName("options");
            entity.Property(e => e.RatePolicy).HasColumnName("rate_policy");
            entity.Property(e => e.AccessToken)
                .HasMaxLength(512)
                .HasColumnName("access_token");
            entity.Property(e => e.RefreshTime).HasColumnName("refresh_time");
            entity.Property(e => e.Enabled).HasColumnName("enabled");
            entity.Property(e => e.Inheritance)
                .HasDefaultValue(true)
                .HasColumnName("inheritance");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CoreOrganization).WithMany(p => p.Apis)
                .HasForeignKey(d => d.CoreOrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_api_core_organization_id_fkey");
        }
    }
}
