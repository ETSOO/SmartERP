using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class CoreDocumentConfiguration : IEntityTypeConfiguration<CoreDocument>
    {
        public void Configure(EntityTypeBuilder<CoreDocument> entity)
        {
            entity.HasKey(e => e.Id).HasName("core_document_pkey");

            entity.ToTable("core_document");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
            entity.Property(e => e.Kind)
                .HasMaxLength(20)
                .HasColumnName("kind");
            entity.Property(e => e.Title)
                .HasMaxLength(128)
                .HasColumnName("title");
            entity.Property(e => e.Parameters)
                .HasColumnType("jsonb")
                .HasColumnName("parameters");
            entity.Property(e => e.Template).HasColumnName("template");
            entity.Property(e => e.RefreshTime).HasColumnName("refresh_time");
        }
    }
}
