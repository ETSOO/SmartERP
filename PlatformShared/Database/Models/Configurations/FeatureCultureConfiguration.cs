using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class FeatureCultureConfiguration : IEntityTypeConfiguration<FeatureCulture>
    {
        public void Configure(EntityTypeBuilder<FeatureCulture> entity)
        {
            entity.HasKey(e => e.Id).HasName("feature_culture_pkey");

            entity.ToTable("feature_culture");

            entity.HasIndex(e => new { e.Key, e.Culture, e.CoreOrganizationId }, "feature_culture_key_culture_core_organization_id_id_title_d_idx")
                .IsUnique()
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Key)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("key");
            entity.Property(e => e.Culture)
                .HasMaxLength(10)
                .IsRequired()
                .HasColumnName("culture");
            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
            entity.Property(e => e.Title)
                .HasMaxLength(256)
                .IsRequired()
                .HasColumnName("title");
            entity.Property(e => e.Description)
                .HasMaxLength(2560)
                .HasColumnName("description");
            entity.Property(e => e.JsonData)
                .HasColumnType("jsonb")
                .HasColumnName("json_data");

            entity.HasOne(d => d.CoreOrganization).WithMany(p => p.FeatureCultures)
                .HasForeignKey(d => d.CoreOrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("feature_culture_core_organization_id_fkey");
        }
    }
}
