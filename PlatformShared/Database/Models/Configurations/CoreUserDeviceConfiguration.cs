using com.etsoo.Database.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class CoreUserDeviceConfiguration : IEntityTypeConfiguration<CoreUserDevice>
    {
        public void Configure(EntityTypeBuilder<CoreUserDevice> entity)
        {
            entity.HasKey(e => e.Id).HasName("core_user_device_pkey");

            entity.ToTable("core_user_device");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1001L)
                .HasColumnName("id");
            entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
            entity.Property(e => e.DeviceType)
                .HasConversion<byte>()
                .HasColumnName("device_type");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.ClientId)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("client_id");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.LastLogin)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_login");
            entity.Property(e => e.Timezone)
                .HasMaxLength(64)
                .HasConversion<TimeZoneInfoToStringConverter>()
                .HasColumnName("timezone");

            entity.HasOne(d => d.CoreUser).WithMany(p => p.CoreUserDevices)
                .HasForeignKey(d => d.CoreUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("core_user_device_core_user_id_fkey");
        }
    }
}
