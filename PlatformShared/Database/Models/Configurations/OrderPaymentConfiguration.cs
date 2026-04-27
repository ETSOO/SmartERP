using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class OrderPaymentConfiguration : IEntityTypeConfiguration<OrderPayment>
    {
        public void Configure(EntityTypeBuilder<OrderPayment> entity)
        {
            entity.HasKey(e => e.Id).HasName("order_payment_pkey");

            entity.ToTable("order_payment");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Kind)
                .HasConversion<byte>()
                .HasColumnName("kind");
            entity.Property(e => e.Title)
                .HasMaxLength(256)
                .HasColumnName("title");
            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
            entity.Property(e => e.IsValid)
                .HasDefaultValue(true)
                .HasColumnName("is_valid");
            entity.Property(e => e.IsOrder)
                .HasDefaultValue(true)
                .HasColumnName("is_order");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
        }
    }
}
