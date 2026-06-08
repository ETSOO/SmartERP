using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class PersonAssetConfiguration : IEntityTypeConfiguration<PersonAsset>
    {
        public void Configure(EntityTypeBuilder<PersonAsset> entity)
        {
            entity.HasKey(e => e.Id).HasName("person_asset_pkey");

            entity.ToTable("person_asset");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.PersonId).HasColumnName("person_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.Sn)
                .HasMaxLength(256)
                .IsRequired()
                .HasColumnName("sn");
            entity.Property(e => e.Description)
                .HasMaxLength(1280)
                .HasColumnName("description");
            entity.Property(e => e.Expiry).HasColumnName("expiry");
            entity.Property(e => e.ExpiryCheck).HasColumnName("expiry_check");
            entity.Property(e => e.Times).HasColumnName("times");
            entity.Property(e => e.Amount)
                .HasColumnType("money")
                .HasColumnName("amount");
            entity.Property(e => e.SensitiveData)
                .HasMaxLength(1280)
                .HasColumnName("sensitive_data");
            entity.Property(e => e.CoreUserId).HasColumnName("core_user_id");
            entity.Property(e => e.Status)
                .HasConversion<byte>()
                .HasDefaultValue(EntityStatus.Normal)
                .HasColumnName("status");
            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");
            entity.Property(e => e.HealthCheckUrl)
                .HasMaxLength(1280)
                .HasColumnName("health_check_url");
            entity.Property(e => e.HealthCheckSchedule)
                .HasColumnName("health_check_schedule");
            entity.OwnsOne(e => e.Data, p => p.ToJson("data"));
            //entity.Property(e => e.Data)
            //    .HasColumnType("jsonb")
            //    .HasColumnName("data");

            entity.HasOne(d => d.Person).WithMany(p => p.OwnedAssets)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_asset_person_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.Assets)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("person_asset_product_id_fkey");

            entity.HasOne(d => d.Supplier).WithMany(p => p.SuppliedAssets)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("person_asset_supplier_id_fkey");
        }
    }
}
