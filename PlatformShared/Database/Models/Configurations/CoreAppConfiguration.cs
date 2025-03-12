using com.etsoo.Database.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlatformShared.Dto;

namespace PlatformShared.Database.Models.Configurations
{
    internal class CoreAppConfiguration : IEntityTypeConfiguration<CoreApp>
    {
        public void Configure(EntityTypeBuilder<CoreApp> entity)
        {
            entity.HasKey(e => e.Id).HasName("core_app_pkey");

            entity.ToTable("core_app");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnName("name");
            entity.Property(e => e.IdentityType)
                .HasConversion<byte>()
                .HasColumnName("identity_type");
            entity.Property(e => e.AppSecret)
                .HasMaxLength(256)
                .HasColumnName("app_secret");
            entity.Property(e => e.Urls)
                .HasColumnType("jsonb")
                .HasConversion(new JsonTypeConverter<AppUrl[]>(PlatformSharedContext.Default.AppUrlArray))
                .HasColumnName("urls");
            entity.Property(e => e.RequireLocalUrl).HasColumnName("require_local_url");
            entity.Property(e => e.Logo)
                .HasMaxLength(256)
                .HasColumnName("logo");
            entity.Property(e => e.IsPublic).HasColumnName("is_public");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.Enabled).HasColumnName("enabled");
        }
    }
}
