using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> entity)
        {
            entity.HasKey(e => e.Id).HasName("product_pkey");

            entity.ToTable("product");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(2001L, null, null, null, null, null)
                .HasColumnName("id");
            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .IsRequired()
                .HasColumnName("name");
            entity.Property(e => e.ForeignName)
                .HasMaxLength(256)
                .HasColumnName("foreign_name");
            entity.Property(e => e.CategoryIds).HasColumnName("category_ids");
            entity.Property(e => e.Description)
                .HasMaxLength(2560)
                .HasColumnName("description");
            entity.Property(e => e.Logo)
                .HasMaxLength(256)
                .HasColumnName("logo");
            entity.Property(e => e.UnitId).HasColumnName("unit_id");
            entity.Property(e => e.MinQty)
                .HasPrecision(6, 2)
                .HasColumnName("min_qty");
            entity.Property(e => e.StepQty)
                .HasPrecision(6, 2)
                .HasColumnName("step_qty");
            entity.Property(e => e.CapQty)
                .HasPrecision(12, 2)
                .HasColumnName("cap_qty");
            entity.Property(e => e.AssetUnit).HasColumnName("asset_unit");
            entity.Property(e => e.AssetQty).HasColumnName("asset_qty");
            entity.Property(e => e.OrderIndex)
                .HasDefaultValue((short)0)
                .HasColumnName("order_index");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.Status)
                .HasConversion<byte>()
                .HasDefaultValue(EntityStatus.Normal)
                .HasColumnName("status");
            entity.Property(e => e.Usage)
                .HasConversion<byte>()
                .HasColumnName("usage");
            entity.Property(e => e.Scope)
                .HasConversion<byte>()
                .HasColumnName("scope");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.QueryKeyword)
                .HasMaxLength(30)
                .HasColumnName("query_keyword");
            entity.Property(e => e.InventoryWay)
                .HasConversion<byte>()
                .HasDefaultValue(ProductInventoryWay.None)
                .HasColumnName("inventory_way");
            entity.Property(e => e.Keywords).HasColumnName("keywords");
        }
    }
}
