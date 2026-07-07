using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class OrderDeliveryConfiguration : IEntityTypeConfiguration<OrderDelivery>
    {
        public void Configure(EntityTypeBuilder<OrderDelivery> entity)
        {
            entity.HasKey(e => e.Id).HasName("delivery_payment_pkey");

            entity.ToTable("order_delivery");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Kind)
                .HasConversion<byte>()
                .HasColumnName("kind");
            entity.Property(e => e.Title)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName("title");
            entity.Property(e => e.Description)
                .HasMaxLength(1280)
                .HasColumnName("description");
            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");
            entity.Property(e => e.IsOrder)
                .HasDefaultValue(true)
                .HasColumnName("is_order");
            entity.Property(e => e.IsValid)
                .HasDefaultValue(true)
                .HasColumnName("is_valid");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
        }
    }
}
