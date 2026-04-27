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
            entity.Property(e => e.CategoryIds).HasColumnName("category_ids");
            entity.Property(e => e.CategoryIdsAll).HasColumnName("category_ids_all");
            entity.Property(e => e.Description)
                .HasMaxLength(2560)
                .HasColumnName("description");
            entity.Property(e => e.Logo)
                .HasMaxLength(256)
                .HasColumnName("logo");
            entity.Property(e => e.IntroductionUrl)
                .HasMaxLength(256)
                .HasColumnName("introduction_url");
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
            entity.Property(e => e.AssetQty).HasColumnName("asset_qty");
            entity.Property(e => e.Validity).HasColumnName("validity");
            entity.Property(e => e.AssignedId).HasColumnName("assigned_id");
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
                .HasConversion<short>()
                .HasColumnName("scope");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.Modifiers)
                .HasColumnType("jsonb")
                .HasColumnName("modifiers");
            entity.Property(e => e.QueryKeyword)
                .HasMaxLength(30)
                .HasColumnName("query_keyword");
            entity.Property(e => e.TaxRate).HasColumnName("tax_rate");
            entity.Property(e => e.Tags).HasColumnName("tags");

            entity.HasOne(d => d.CoreOrganization).WithMany(p => p.Products)
                .HasForeignKey(d => d.CoreOrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_core_organization_id_fkey");

            entity.HasOne(d => d.Unit).WithMany(p => p.Products)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_unit_id_fkey");
        }
    }
}
