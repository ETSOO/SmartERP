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
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");
            entity.Property(e => e.Description)
                .HasMaxLength(2560)
                .HasColumnName("description");
            entity.Property(e => e.AssignedId)
                .HasMaxLength(20)
                .HasColumnName("assigned_id");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("currency");
            entity.Property(e => e.RetailPrice)
                .HasColumnType("money")
                .HasColumnName("retail_price");
            entity.Property(e => e.UpdatedTime)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_time");
        }
    }
}
