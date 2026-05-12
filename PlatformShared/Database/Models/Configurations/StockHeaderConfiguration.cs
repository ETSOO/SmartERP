using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class StockHeaderConfiguration : IEntityTypeConfiguration<StockHeader>
    {
        public void Configure(EntityTypeBuilder<StockHeader> entity)
        {
            entity.HasKey(e => e.Id).HasName("stock_header_pkey");

            entity.ToTable("stock_header");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.Kind)
                .HasConversion<byte>()
                .HasColumnName("kind");
            entity.Property(e => e.PersonId).HasColumnName("person_id");
            entity.Property(e => e.LocationFromId).HasColumnName("location_from_id");
            entity.Property(e => e.LocationToId).HasColumnName("location_to_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Title)
                .HasMaxLength(128)
                .HasColumnName("title");
            entity.Property(e => e.Description)
                .HasMaxLength(1280)
                .HasColumnName("description");
            entity.Property(e => e.TrackingNumber)
                .HasMaxLength(20)
                .HasColumnName("tracking_number");
            entity.Property(e => e.OrderIds).HasColumnName("order_ids");
            entity.Property(e => e.ReceiptTime).HasColumnName("receipt_time");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");

            entity.HasOne(d => d.LocationFrom).WithMany(p => p.StockFroms)
                .HasForeignKey(d => d.LocationFrom)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_header_location_from_id_fkey");

            entity.HasOne(d => d.LocationTo).WithMany(p => p.StockTos)
                .HasForeignKey(d => d.LocationTo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_header_location_to_id_fkey");

            entity.HasOne(d => d.Person).WithMany(p => p.Stocks)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_header_person_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserStocks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_header_user_id_fkey");
        }
    }
}
