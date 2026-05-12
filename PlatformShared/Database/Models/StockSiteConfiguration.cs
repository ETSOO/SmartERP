using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models
{
    internal class StockSiteConfiguration : IEntityTypeConfiguration<StockSite>
    {
        public void Configure(EntityTypeBuilder<StockSite> entity)
        {
            entity.HasKey(e => e.Id).HasName("stock_site_pkey");

            entity.ToTable("stock_site");

            entity.HasIndex(e => new { e.ProductId, e.LocationId }, "stock_site_product_id_location_id_qty_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.Qty)
                .HasPrecision(12, 2)
                .HasColumnName("qty");
            entity.Property(e => e.RefreshTime)
                .HasDefaultValueSql("now()")
                .HasColumnName("refresh_time");

            entity.HasOne(d => d.Location).WithMany(p => p.StockSites)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("stock_site_location_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.StockSites)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_site_product_id_fkey");
        }
    }
}
