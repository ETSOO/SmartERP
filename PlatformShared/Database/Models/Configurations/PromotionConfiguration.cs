using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlatformShared.Dto;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> entity)
        {
            entity.HasKey(e => e.Id).HasName("promotion_pkey");

            entity.ToTable("promotion");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(2001L, null, null, null, null, null)
                .HasColumnName("id");
            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
            entity.Property(e => e.Title)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName("title");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired()
                .HasColumnName("currency");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductCategoryId).HasColumnName("product_category_id");
            entity.Property(e => e.PersonId).HasColumnName("person_id");
            entity.Property(e => e.PersonCategoryId).HasColumnName("person_category_id");
            entity.Property(e => e.Code)
                .HasConversion<PromotionCodeConverter>()
                .HasColumnName("code");
            entity.Property(e => e.MinAmount)
                .HasColumnType("money")
                .HasColumnName("min_amount");
            entity.Property(e => e.Discount).HasColumnName("discount");
            entity.Property(e => e.ValidStart).HasColumnName("valid_start");
            entity.Property(e => e.ValidEnd).HasColumnName("valid_end");
            entity.Property(e => e.Coupons).HasColumnName("coupons");
            entity.Property(e => e.Stackable).HasColumnName("stackable");
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
        }
    }
}
