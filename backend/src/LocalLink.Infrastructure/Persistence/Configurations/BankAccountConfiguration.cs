using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("BankAccounts", t =>
        {
            t.HasCheckConstraint("CK_BankAccounts_Balance_NonNegative", "[Balance] >= 0");
        });

        builder.HasKey(ba => ba.Id);

        builder.Property(ba => ba.CustomerId)
            .IsRequired();

        builder.Property(ba => ba.AccountNumber)
            .IsRequired()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.HasIndex(ba => ba.AccountNumber)
            .IsUnique();

        builder.Property(ba => ba.AccountName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(ba => ba.AccountType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(ba => ba.Balance)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ba => ba.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength()
            .IsUnicode(false);

        builder.Property(ba => ba.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(ba => ba.RowVersion)
            .IsRowVersion();

        builder.Property(ba => ba.CreatedAtUtc)
            .IsRequired();

        builder.Property(ba => ba.UpdatedAtUtc)
            .IsRequired(false);

        builder.HasIndex(ba => ba.CustomerId);
        builder.HasIndex(ba => ba.Status);

        builder.HasOne(ba => ba.Customer)
            .WithMany(c => c.BankAccounts)
            .HasForeignKey(ba => ba.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ba => ba.BeneficiaryRecords)
            .WithOne(b => b.BeneficiaryAccount)
            .HasForeignKey(b => b.BeneficiaryAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ba => ba.SourceTransactions)
            .WithOne(t => t.SourceAccount)
            .HasForeignKey(t => t.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ba => ba.DestinationTransactions)
            .WithOne(t => t.DestinationAccount)
            .HasForeignKey(t => t.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ba => ba.SourceTransfers)
            .WithOne(t => t.SourceAccount)
            .HasForeignKey(t => t.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ba => ba.DestinationTransfers)
            .WithOne(t => t.DestinationAccount)
            .HasForeignKey(t => t.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ba => ba.Payments)
            .WithOne(p => p.Account)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
