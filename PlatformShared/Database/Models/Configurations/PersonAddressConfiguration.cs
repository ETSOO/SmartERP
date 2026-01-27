using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PersonAddressConfiguration : IEntityTypeConfiguration<PersonAddress>
    {
        public void Configure(EntityTypeBuilder<PersonAddress> entity)
        {
            entity.HasKey(e => e.Id).HasName("person_address_pkey");

            entity.ToTable("person_address");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1001L, null, null, null, null, null)
                .HasColumnName("id");
            entity.Property(e => e.PersonId)
                .HasColumnName("person_id");
            entity.Property(e => e.Kind)
                .HasConversion<byte>()
                .HasColumnName("kind");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.PlaceId)
                .HasMaxLength(30)
                .HasColumnName("place_id");
            entity.Property(e => e.Region)
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("region");
            entity.Property(e => e.State)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("state");
            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.District)
                .HasMaxLength(50)
                .HasColumnName("district");
            entity.Property(e => e.Route)
                .HasMaxLength(128)
                .HasColumnName("route");
            entity.Property(e => e.Street)
                .HasMaxLength(128)
                .HasColumnName("street");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(10)
                .HasColumnName("postal_code");
            entity.Property(e => e.FormattedAddress)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("formatted_address");
            entity.Property(e => e.Location).HasColumnName("location");
            entity.Property(e => e.Provider)
                .HasConversion<byte>()
                .HasColumnName("provider");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");

            entity.HasOne(d => d.Parent).WithMany(p => p.Locations)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("person_address_parent_id_fkey");

            entity.HasOne(d => d.Person).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_address_person_id_fkey");
        }
    }
}
