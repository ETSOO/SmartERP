using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class FeatureKeywordConfiguration : IEntityTypeConfiguration<FeatureKeyword>
    {
        public void Configure(EntityTypeBuilder<FeatureKeyword> entity)
        {
            entity.HasKey(e => e.Id).HasName("feature_keyword_pkey");

            entity.ToTable("feature_keyword");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Tag)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName("tag");
            entity.Property(e => e.CoreOrganizationId)
                .IsRequired()
                .HasColumnName("core_organization_id");
            entity.Property(e => e.Kind)
                .IsRequired()
                .HasConversion<short>()
                .HasColumnName("kind");
            entity.Property(e => e.Total)
                .IsRequired()
                .HasColumnName("total");
        }
    }
}
