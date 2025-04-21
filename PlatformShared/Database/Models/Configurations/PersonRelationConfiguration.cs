using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PersonRelationConfiguration : IEntityTypeConfiguration<PersonRelation>
    {
        public void Configure(EntityTypeBuilder<PersonRelation> entity)
        {
            entity.HasKey(e => new { e.PersonId, e.ContactId }).HasName("person_relation_pkey");

            entity.ToTable("person_relation");

            entity.Property(e => e.PersonId).HasColumnName("person_id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.RelationType)
                .HasConversion<byte>()
                .HasColumnName("relation_type");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.Description)
                .HasMaxLength(128)
                .HasColumnName("description");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");

            entity.HasOne(d => d.Contact).WithMany(p => p.ContactOwners)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_relation_contact_id_fkey");

            entity.HasOne(d => d.Person).WithMany(p => p.Contacts)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_relation_person_id_fkey");
        }
    }
}
