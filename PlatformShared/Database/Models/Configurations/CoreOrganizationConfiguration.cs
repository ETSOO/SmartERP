using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class CoreOrganizationConfiguration : IEntityTypeConfiguration<CoreOrganization>
    {
        public void Configure(EntityTypeBuilder<CoreOrganization> entity)
        {
            entity.HasKey(e => e.Id).HasName("core_organization_pkey");

            entity.ToTable("core_organization");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1001L)
                .HasColumnName("id");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.Brand)
                .HasMaxLength(30)
                .HasColumnName("brand");
            entity.Property(e => e.Logo)
                .HasMaxLength(256)
                .HasColumnName("logo");
            entity.Property(e => e.Pin)
                .HasMaxLength(20)
                .HasColumnName("pin");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.Status)
                .HasConversion<byte>()
                .HasDefaultValue(EntityStatus.Normal)
                .HasColumnName("status");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.QueryKeyword)
                .HasMaxLength(30)
                .HasColumnName("query_keyword");
            entity.Property(e => e.Region)
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength(true)
                .HasColumnName("region");
            entity.Property(e => e.TimeZone)
                .IsRequired()
                .HasMaxLength(64)
                .HasColumnName("time_zone");
            entity.Property(e => e.Slogan)
                .HasMaxLength(128)
                .HasColumnName("slogan");

            entity.HasOne(d => d.Owner).WithMany(u => u.OwnedOrganizations)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_organization_owner_id_fkey");

            entity.HasOne(d => d.Parent).WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("core_organization_parent_id_fkey");
        }
    }
}
