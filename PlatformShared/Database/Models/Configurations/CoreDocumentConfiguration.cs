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
            entity.Property(e => e.Cultures)
                .HasColumnType("character varying(20)[]")
                .HasColumnName("cultures");

            entity.HasOne(d => d.CoreOrganization).WithMany(p => p.Documents)
                .HasForeignKey(d => d.CoreOrganizationId)
                .HasConstraintName("core_document_core_organization_id_fkey");
        }
    }
}
