using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.ToTable("LoanApplications", t =>
        {
            t.HasCheckConstraint("CK_LoanApplications_RequestedAmount_Positive", "[RequestedAmount] > 0");
            t.HasCheckConstraint("CK_LoanApplications_ApprovedAmount_Positive", "[ApprovedAmount] IS NULL OR [ApprovedAmount] > 0");
            t.HasCheckConstraint("CK_LoanApplications_InterestRate_NonNegative", "[AnnualInterestRate] >= 0");
            t.HasCheckConstraint("CK_LoanApplications_Term_Valid", "[TermMonths] BETWEEN 6 AND 60");
            t.HasCheckConstraint("CK_LoanApplications_MonthlyIncome_Positive", "[MonthlyIncome] > 0");
        });

        builder.HasKey(loan => loan.Id);

        builder.Property(loan => loan.ApplicationNumber)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.HasIndex(loan => loan.ApplicationNumber)
            .IsUnique();

        builder.Property(loan => loan.RequestedAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(loan => loan.ApprovedAmount)
            .HasPrecision(18, 2);

        builder.Property(loan => loan.AnnualInterestRate)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(loan => loan.MonthlyIncome)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(loan => loan.Purpose)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(loan => loan.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength()
            .IsUnicode(false);

        builder.Property(loan => loan.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(loan => loan.ReviewNote)
            .HasMaxLength(500);

        builder.Property(loan => loan.SubmittedAtUtc)
            .IsRequired();

        builder.Property(loan => loan.CreatedAtUtc)
            .IsRequired();

        builder.Property(loan => loan.UpdatedAtUtc)
            .IsRequired(false);

        builder.HasIndex(loan => new { loan.CustomerId, loan.Status });
        builder.HasIndex(loan => loan.Status);
        builder.HasIndex(loan => loan.DisbursementAccountId);
        builder.HasIndex(loan => loan.ReviewedByUserId);
        builder.HasIndex(loan => loan.DisbursementTransactionId)
            .IsUnique()
            .HasFilter("[DisbursementTransactionId] IS NOT NULL");

        builder.HasOne(loan => loan.Customer)
            .WithMany(customer => customer.LoanApplications)
            .HasForeignKey(loan => loan.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(loan => loan.DisbursementAccount)
            .WithMany(account => account.LoanApplications)
            .HasForeignKey(loan => loan.DisbursementAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(loan => loan.ReviewedByUser)
            .WithMany()
            .HasForeignKey(loan => loan.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(loan => loan.DisbursementTransaction)
            .WithOne(transaction => transaction.LoanApplication)
            .HasForeignKey<LoanApplication>(loan => loan.DisbursementTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
