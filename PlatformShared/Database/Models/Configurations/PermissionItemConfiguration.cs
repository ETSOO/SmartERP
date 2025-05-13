using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PermissionItemConfiguration : IEntityTypeConfiguration<PermissionItem>
    {
        public void Configure(EntityTypeBuilder<PermissionItem> entity)
        {
            entity.HasKey(e => e.Id).HasName("permission_item_pkey");

            entity.ToTable("permission_item");

            entity.HasIndex(e => new { e.Module, e.Name }, "permission_item_module_name_id_idx")
                .IsUnique()
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Module)
                .IsRequired()
                .HasConversion<short>()
                .HasColumnName("module");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsRequired()
                .HasColumnName("name");
        }
    }
}
