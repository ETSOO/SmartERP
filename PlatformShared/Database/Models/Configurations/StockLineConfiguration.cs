using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class StockLineConfiguration : IEntityTypeConfiguration<StockLine>
    {
        public void Configure(EntityTypeBuilder<StockLine> entity)
        {
            entity.HasKey(e => e.Id).HasName("stock_line_pkey");

            entity.ToTable("stock_line");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.StockId).HasColumnName("stock_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Qty)
                .HasPrecision(12, 2)
                .HasColumnName("qty");
            entity.Property(e => e.OrderLineId).HasColumnName("order_line_id");

            entity.HasOne(d => d.Location).WithMany(p => p.StockLines)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_line_location_id_fkey");

            entity.HasOne(d => d.OrderLine).WithMany(p => p.StockLines)
                .HasForeignKey(d => d.OrderLineId)
                .HasConstraintName("stock_line_order_line_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.StockLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_line_product_id_fkey");

            entity.HasOne(d => d.Stock).WithMany(p => p.Lines)
                .HasForeignKey(d => d.StockId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_line_stock_id_fkey");
        }
    }
}
