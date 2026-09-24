using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlatformShared.Database.Models.Configurations
{
    internal class FinanceTransactionConfiguration : IEntityTypeConfiguration<FinanceTransaction>
    {
        public void Configure(EntityTypeBuilder<FinanceTransaction> entity)
        {
            entity.HasKey(e => e.Id).HasName("finance_transaction_pkey");

            entity.ToTable("finance_transaction");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");

            entity.Property(e => e.CoreOrganizationId).HasColumnName("core_organization_id");

            entity.Property(e => e.Kind)
                .HasConversion<short>()
                .HasColumnName("kind");

            entity.Property(e => e.AccountId).HasColumnName("account_id");

            entity.Property(e => e.Title)
                .HasMaxLength(256)
                .IsRequired()
                .HasColumnName("title");

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");

            entity.Property(e => e.OrderId).HasColumnName("order_id");

            entity.Property(e => e.ReferenceId)
                .HasMaxLength(30)
                .HasColumnName("reference_id");

            entity.Property(e => e.TargetPersonId).HasColumnName("target_person_id");

            entity.Property(e => e.TargetAccountId).HasColumnName("target_account_id");

            entity.Property(e => e.AuthorId).HasColumnName("author_id");

            entity.Property(e => e.Creation)
                .HasDefaultValueSql("now()")
                .HasColumnName("creation");


            entity.HasOne(d => d.Account).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("finance_transaction_account_id_fkey");

            entity.HasOne(d => d.Author).WithMany(p => p.FinanceTransactions)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("finance_transaction_author_id_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.FinanceTransactions)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("finance_transaction_order_id_fkey");

            entity.HasOne(d => d.TargetAccount).WithMany(p => p.TargetTransactions)
                .HasForeignKey(d => d.TargetAccountId)
                .HasConstraintName("finance_transaction_target_account_id_fkey");

            entity.HasOne(d => d.TargetPerson).WithMany(p => p.TargetTransactions)
                .HasForeignKey(d => d.TargetPersonId)
                .HasConstraintName("finance_transaction_target_person_id_fkey");
        }
    }
}
