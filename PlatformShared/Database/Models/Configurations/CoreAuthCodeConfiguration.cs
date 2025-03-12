using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PlatformShared.Database.Models.Configurations
{
    internal class CoreAuthCodeConfiguration : IEntityTypeConfiguration<CoreAuthCode>
    {
        public void Configure(EntityTypeBuilder<CoreAuthCode> entity)
        {
            entity.HasKey(e => e.Id).HasName("core_auth_code_pkey");

            entity.ToTable("core_auth_code");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
            entity.Property(e => e.Action)
                .IsRequired()
                .HasConversion<short>()
                .HasColumnName("action");
            entity.Property(e => e.Openid)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("openid");
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("code");
            entity.Property(e => e.Expiry).HasColumnName("expiry");
            entity.Property(e => e.Ip)
                .IsRequired()
                .HasMaxLength(45)
                .HasConversion<IPAddressToStringConverter>()
                .HasColumnName("ip");
            entity.Property(e => e.Times)
                .HasDefaultValue((short)0)
                .HasColumnName("times");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");

            entity.HasOne(d => d.CoreUser).WithMany(p => p.CoreUserAuthCodes)
                .HasForeignKey(d => d.CoreUserId)
                .HasConstraintName("core_auth_code_core_user_id_fkey");
        }
    }
}
