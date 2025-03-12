using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class CoreUserIdentifierConfiguration : IEntityTypeConfiguration<CoreUserIdentifier>
    {
        public void Configure(EntityTypeBuilder<CoreUserIdentifier> entity)
        {
            entity.HasKey(e => e.Id).HasName("core_user_identifier_pkey");

            entity.ToTable("core_user_identifier");

            entity.HasIndex(e => new { e.Type, e.Value }, "core_user_identifier_type_value_id_core_user_id_ref_type_idx")
                .IsUnique()
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "false");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("value");
            entity.Property(e => e.RefType).HasColumnName("ref_type");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");

            entity.HasOne(d => d.CoreUser).WithMany(p => p.CoreUserIdentifiers)
                .HasForeignKey(d => d.CoreUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_user_identifier_core_user_id_fkey");
        }
    }
}
