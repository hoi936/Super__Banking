using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class TermDepositConfiguration : IEntityTypeConfiguration<TermDeposit>
{
    public void Configure(EntityTypeBuilder<TermDeposit> builder)
    {
        builder.ToTable("TermDeposits", t =>
        {
            t.HasCheckConstraint("CK_TermDeposits_Principal_Positive", "[PrincipalAmount] > 0");
            t.HasCheckConstraint("CK_TermDeposits_Tenor_Allowed", "[TenorMonths] IN (1, 3, 12)");
            t.HasCheckConstraint("CK_TermDeposits_InterestRate_NonNegative", "[AnnualInterestRate] >= 0");
            t.HasCheckConstraint("CK_TermDeposits_ExpectedInterest_NonNegative", "[ExpectedInterestAmount] >= 0");
        });

        builder.HasKey(td => td.Id);

        builder.Property(td => td.DepositNumber)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.HasIndex(td => td.DepositNumber)
            .IsUnique();

        builder.Property(td => td.PrincipalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(td => td.AnnualInterestRate)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(td => td.TenorMonths)
            .IsRequired();

        builder.Property(td => td.ExpectedInterestAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(td => td.PaidInterestAmount)
            .HasPrecision(18, 2);

        builder.Property(td => td.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength()
            .IsUnicode(false);

        builder.Property(td => td.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(td => td.IdempotencyKey)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.HasIndex(td => td.IdempotencyKey)
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL");

        builder.Property(td => td.OpenedAtUtc)
            .IsRequired();

        builder.Property(td => td.MaturityDateUtc)
            .IsRequired();

        builder.Property(td => td.ClosedAtUtc)
            .IsRequired(false);

        builder.Property(td => td.CreatedAtUtc)
            .IsRequired();

        builder.Property(td => td.UpdatedAtUtc)
            .IsRequired(false);

        builder.Property(td => td.RowVersion)
            .IsRowVersion();

        builder.HasIndex(td => new { td.CustomerId, td.Status });
        builder.HasIndex(td => td.MaturityDateUtc);

        builder.HasOne(td => td.Customer)
            .WithMany()
            .HasForeignKey(td => td.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(td => td.SourceAccount)
            .WithMany(ba => ba.TermDeposits)
            .HasForeignKey(td => td.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(td => td.OpeningTransaction)
            .WithOne()
            .HasForeignKey<TermDeposit>(td => td.OpeningTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(td => td.MaturityTransaction)
            .WithOne()
            .HasForeignKey<TermDeposit>(td => td.MaturityTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
