using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class ExternalTransferConfiguration : IEntityTypeConfiguration<ExternalTransfer>
{
    public void Configure(EntityTypeBuilder<ExternalTransfer> builder)
    {
        builder.ToTable("ExternalTransfers", t =>
        {
            t.HasCheckConstraint("CK_ExternalTransfers_Amount_Positive", "[Amount] > 0");
            t.HasCheckConstraint("CK_ExternalTransfers_Fee_NonNegative", "[FeeAmount] >= 0");
            t.HasCheckConstraint("CK_ExternalTransfers_DestinationType_Allowed", "[DestinationType] IN ('ACCOUNT', 'CARD')");
        });

        builder.HasKey(et => et.Id);

        builder.Property(et => et.TransactionId)
            .IsRequired();

        builder.HasIndex(et => et.TransactionId)
            .IsUnique();

        builder.Property(et => et.SourceAccountId)
            .IsRequired();

        builder.Property(et => et.ExternalBankCode)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(et => et.ExternalBankName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(et => et.DestinationAccountNumber)
            .IsRequired()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(et => et.DestinationAccountName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(et => et.DestinationType)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(et => et.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(et => et.FeeAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(et => et.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength()
            .IsUnicode(false);

        builder.Property(et => et.Description)
            .HasMaxLength(500);

        builder.Property(et => et.IdempotencyKey)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.HasIndex(et => et.IdempotencyKey)
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL");

        builder.Property(et => et.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(et => et.CreatedAtUtc)
            .IsRequired();

        builder.Property(et => et.CompletedAtUtc)
            .IsRequired(false);

        builder.HasIndex(et => new { et.SourceAccountId, et.CreatedAtUtc });
        builder.HasIndex(et => new { et.ExternalBankCode, et.CreatedAtUtc });
        builder.HasIndex(et => et.Status);

        builder.HasOne(et => et.Transaction)
            .WithOne(t => t.ExternalTransfer)
            .HasForeignKey<ExternalTransfer>(et => et.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(et => et.SourceAccount)
            .WithMany(ba => ba.ExternalTransfers)
            .HasForeignKey(et => et.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
