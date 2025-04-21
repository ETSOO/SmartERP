using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PersonInfoConfiguration : IEntityTypeConfiguration<PersonInfo>
    {
        public void Configure(EntityTypeBuilder<PersonInfo> entity)
        {
            entity.HasKey(e => e.Id).HasName("person_info_pkey");

            entity.ToTable("person_info");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.PersonId).HasColumnName("person_id");
            entity.Property(e => e.Kind)
                .HasConversion<byte>()
                .HasColumnName("kind");
            entity.Property(e => e.Identifier)
                .HasMaxLength(256)
                .IsRequired()
                .HasColumnName("identifier");
            entity.Property(e => e.Description)
                .HasMaxLength(256)
                .HasColumnName("description");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
            entity.Property(e => e.Subscribed).HasColumnName("subscribed");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");

            entity.HasOne(d => d.Person).WithMany(p => p.Infos)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_info_person_id_fkey");
        }
    }
}
