using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class ProductBomConfiguration : IEntityTypeConfiguration<ProductBom>
    {
        public void Configure(EntityTypeBuilder<ProductBom> entity)
        {
            entity.HasKey(e => e.Id).HasName("product_bom_pkey");

            entity.ToTable("product_bom");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1000L, null, null, null, null, null)
                .HasColumnName("id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Qty)
                .HasPrecision(15, 2)
                .HasColumnName("qty");

            entity.HasOne(d => d.Parent).WithMany(p => p.Boms)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("product_bom_parent_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.BomParents)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_bom_product_id_fkey");
        }
    }
}
