using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
    {
        public void Configure(EntityTypeBuilder<OrderLine> entity)
        {
            entity.HasKey(e => e.Id).HasName("order_line_pkey");

            entity.ToTable("order_line");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("title");
            entity.Property(e => e.Description)
                .HasMaxLength(1280)
                .HasColumnName("description");
            entity.Property(e => e.OriginalPrice)
                .HasColumnType("money")
                .HasColumnName("original_price");
            entity.Property(e => e.CostPrice)
                .HasColumnType("money")
                .HasColumnName("cost_price");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
            entity.Property(e => e.Qty)
                .HasPrecision(12, 2)
                .HasColumnName("qty");
            entity.Property(e => e.AssetQty).HasColumnName("asset_qty");
            entity.Property(e => e.Amount)
                .HasColumnType("money")
                .HasColumnName("amount");
            entity.Property(e => e.Discount)
                .HasColumnType("money")
                .HasColumnName("discount");
            entity.OwnsMany(e => e.Promotions,
                p => p.ToJson("promotions"));
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.AssetId).HasColumnName("asset_id");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.Status)
                .HasConversion<byte>()
                .HasDefaultValue(EntityStatus.Normal)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");

            entity.HasOne(d => d.Asset).WithMany(p => p.OrderLines)
                .HasForeignKey(d => d.AssetId)
                .HasConstraintName("order_line_asset_id_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderLines)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_line_order_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_line_product_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.SupplierOrderLines)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("order_line_user_id_fkey");

            entity.HasOne(d => d.Supplier).WithMany(p => p.OrderLines)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("order_line_supplier_id_fkey");
        }
    }
}
