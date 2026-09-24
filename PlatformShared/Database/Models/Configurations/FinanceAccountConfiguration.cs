using com.etsoo.CoreFramework.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class FinanceAccountConfiguration : IEntityTypeConfiguration<FinanceAccount>
    {
        public void Configure(EntityTypeBuilder<FinanceAccount> entity)
        {
            entity.HasKey(e => e.Id).HasName("finance_account_pkey");

            entity.ToTable("finance_account");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasIdentityOptions(1001L, null, null, null, null, null)
                .HasColumnName("id");

            entity.Property(e => e.PersonId).HasColumnName("person_id");

            entity.Property(e => e.Kind)
                .HasConversion<short>()
                .HasColumnName("kind");

            entity.Property(e => e.Bank)
                .HasMaxLength(128)
                .IsRequired()
                .HasColumnName("bank");

            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("currency");

            entity.Property(e => e.AccountNumber)
                .HasMaxLength(20)
                .IsRequired()
                .HasColumnName("account_number");

            entity.Property(e => e.Swift)
                .HasMaxLength(256)
                .HasColumnName("swift");

            entity.Property(e => e.Description)
                .HasMaxLength(128)
                .HasColumnName("description");

            entity.Property(e => e.Balance)
                .HasPrecision(18, 2)
                .HasColumnName("balance");

            entity.Property(e => e.Status)
                .HasConversion<short>()
                .HasDefaultValue(EntityStatus.Normal)
                .HasColumnName("status");

            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");

            entity.Property(e => e.Expiry)
                .HasColumnName("expiry");

            entity.Property(e => e.ProductId)
                .HasColumnName("product_id");

            entity.Property(e => e.RefreshTime)
                .HasColumnName("refresh_time");

            entity.HasOne(d => d.Person).WithMany(p => p.FinanceAccounts)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("finance_account_person_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.FinanceAccounts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("finance_account_product_id_fkey");
        }
    }
}
