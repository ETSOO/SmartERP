using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PersonProductConfiguration : IEntityTypeConfiguration<PersonProduct>
    {
        public void Configure(EntityTypeBuilder<PersonProduct> entity)
        {
            entity.HasKey(e => new { e.PersonId, e.ProductId }).HasName("person_product_pkey");

            entity.ToTable("person_product");

            entity.Property(e => e.PersonId).HasColumnName("person_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.AssignedId)
                .HasMaxLength(20)
                .HasColumnName("assigned_id");
            // Complex types (EF10+) and should not include 'HasColumnType("jsonb")'
            entity.ComplexProperty(e => e.JsonData, j => j.ToJson("json_data"));
            /*
            // Owned entities
            entity.OwnsOne(e => e.JsonData, j =>
            {
                j.ToJson("json_data").HasColumnType("jsonb");
                j.OwnsMany(d => d.Cultures);
                j.OwnsMany(d => d.Prices);
            });
            */
            entity.Property(e => e.UpdatedTime)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_time");

            entity.HasOne(d => d.Product).WithMany(p => p.PersonProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_product_product_id_fkey");
        }
    }
}
