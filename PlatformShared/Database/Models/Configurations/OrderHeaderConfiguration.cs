using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class OrderHeaderConfiguration : IEntityTypeConfiguration<OrderHeader>
    {
        public void Configure(EntityTypeBuilder<OrderHeader> entity)
        {
            entity.HasKey(e => e.Id).HasName("order_header_pkey");

            entity.ToTable("order_header");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(2001L, null, null, null, null, null)
                .HasColumnName("id");
            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
            entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
            entity.Property(e => e.Source)
                .HasMaxLength(20)
                .HasColumnName("source");
            entity.Property(e => e.SourceId)
                .HasMaxLength(50)
                .HasColumnName("source_id");
            entity.Property(e => e.AssignedId)
                .HasMaxLength(20)
                .HasColumnName("assigned_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName("title");
            entity.Property(e => e.Description)
                .HasMaxLength(1280)
                .HasColumnName("description");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("currency");
            entity.Property(e => e.Amount)
                .HasColumnType("money")
                .HasColumnName("amount");
            entity.Property(e => e.PaidAmount)
                .HasColumnType("money")
                .HasColumnName("paid_amount");
            entity.Property(e => e.Discount)
                .HasColumnType("money")
                .HasColumnName("discount");
            entity.Property(e => e.LineDiscount)
                .HasColumnType("money")
                .HasColumnName("line_discount");
            entity.Property(e => e.Lines).HasColumnName("lines");
            entity.Property(e => e.Items)
                .HasPrecision(12, 2)
                .HasColumnName("items");
            entity.OwnsMany(e => e.Promotions,
                p => p.ToJson("promotions")
                    .HasColumnType("jsonb")
                    .Property(p => p.Code).HasConversion<PromotionCodeConverter>());
            entity.Property(e => e.Culture)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("culture");
            entity.Property(e => e.PaymentId)
                .HasConversion<byte>()
                .HasColumnName("payment_id");
            entity.Property(e => e.PaymentInstruction)
                .HasMaxLength(256)
                .HasColumnName("payment_instruction");
            entity.Property(e => e.DeliveryId)
                .HasConversion<byte>()
                .HasColumnName("delivery_id");
            entity.Property(e => e.AddressId)
                .HasConversion<byte>()
                .HasColumnName("address_id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.DeliveryInstruction)
                .HasMaxLength(256)
                .HasColumnName("delivery_instruction");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
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
