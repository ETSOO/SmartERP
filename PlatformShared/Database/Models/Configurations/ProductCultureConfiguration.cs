using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class ProductCultureConfiguration : IEntityTypeConfiguration<ProductCulture>
    {
        public void Configure(EntityTypeBuilder<ProductCulture> entity)
        {
            entity.HasKey(e => new { e.ProductId, e.Culture }).HasName("product_culture_pkey");

            entity.ToTable("product_culture");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Culture)
                .HasMaxLength(10)
                .HasColumnName("culture");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .IsRequired()
                .HasColumnName("name");
            entity.Property(e => e.Description)
                .HasMaxLength(2560)
                .HasColumnName("description");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
        }
    }
}
