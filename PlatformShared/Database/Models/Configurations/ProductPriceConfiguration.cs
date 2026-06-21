using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
    {
        public void Configure(EntityTypeBuilder<ProductPrice> entity)
        {
            entity.HasKey(e => new { e.ProductId, e.Currency }).HasName("product_price_pkey");

            entity.ToTable("product_price");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("currency");
            entity.Property(e => e.RetailPrice)
                .HasColumnName("retail_price");
            entity.Property(e => e.PromotionPrice)
                .HasColumnName("promotion_price");
            entity.Property(e => e.ChannelPrice)
                .HasColumnName("channel_price");
            entity.Property(e => e.CostPrice)
                .HasColumnName("cost_price");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");

            entity.HasOne(d => d.Product).WithMany(p => p.Prices)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_price_product_id_fkey");
        }
    }
}
