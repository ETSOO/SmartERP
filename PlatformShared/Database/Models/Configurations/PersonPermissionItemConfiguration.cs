using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PersonPermissionItemConfiguration : IEntityTypeConfiguration<PersonPermissionItem>
    {
        public void Configure(EntityTypeBuilder<PersonPermissionItem> entity)
        {
            entity.HasKey(e => new { e.PersonId, e.PermissionItemId }).HasName("person_permission_item_pkey");

            entity.ToTable("person_permission_item");

            entity.Property(e => e.PersonId).HasColumnName("person_id");
            entity.Property(e => e.PermissionItemId).HasColumnName("permission_item_id");

            entity.HasOne(d => d.PermissionItem).WithMany(p => p.PersonPermissionItems)
                .HasForeignKey(d => d.PermissionItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_permission_item_permission_item_id_fkey");

            entity.HasOne(d => d.Person).WithMany(p => p.PermissionItems)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_permission_item_person_id_fkey");
        }
    }
}
