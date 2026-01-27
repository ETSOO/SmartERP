using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class SettingCrmConfiguration : IEntityTypeConfiguration<SettingCrm>
    {
        public void Configure(EntityTypeBuilder<SettingCrm> entity)
        {
            entity.HasKey(e => e.Id).HasName("setting_crm_pkey");

            entity.ToTable("setting_crm");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.PersonId).HasColumnName("person_id");
            entity.Property(e => e.MainCustomerType)
                .HasConversion<short>()
                .HasColumnName("main_customer_type");
            entity.Property(e => e.Currencies)
                .HasColumnType("character(3)[]")
                .HasColumnName("currencies");
            entity.Property(e => e.SupplierCurrencies)
                .HasColumnType("character(3)[]")
                .HasColumnName("supplier_currencies");
            entity.Property(e => e.Cultures)
                .HasColumnType("character varying(10)[]")
                .HasColumnName("cultures");
            entity.Property(e => e.HasInventory).HasColumnName("has_inventory");
            entity.Property(e => e.TaxRate).HasColumnName("tax_rate");

            entity.HasOne(d => d.Organization).WithOne(p => p.SettingCrm)
                .HasForeignKey<SettingCrm>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("setting_crm_id_fkey");

            entity.HasOne(d => d.Person).WithOne(p => p.SettingCrm)
                .HasForeignKey<SettingCrm>(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("setting_crm_person_id_fkey");
        }
    }
}
