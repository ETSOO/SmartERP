using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PermissionGroupConfiguration : IEntityTypeConfiguration<PermissionGroup>
    {
        public void Configure(EntityTypeBuilder<PermissionGroup> entity)
        {
            entity.HasKey(e => e.Id).HasName("permission_group_pkey");

            entity.ToTable("permission_group");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName("name");
            entity.Property(e => e.Roles)
                .IsRequired()
                .HasConversion<short>()
                .HasColumnName("roles");
            entity.Property(e => e.Items)
                .IsRequired()
                .HasColumnName("items");
            entity.Property(e => e.CoreOrganizationId)
                .HasColumnName("core_organization_id");

            entity.HasOne(d => d.CoreOrganization).WithMany(p => p.PermissionGroups)
                .HasForeignKey(d => d.CoreOrganizationId)
                .HasConstraintName("permission_group_core_organization_id_fkey");
        }
    }
}
