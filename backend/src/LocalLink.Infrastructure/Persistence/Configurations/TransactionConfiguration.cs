using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions", t =>
        {
            t.HasCheckConstraint("CK_Transactions_Amount_Positive", "[Amount] > 0");
        });

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.HasIndex(t => t.ReferenceNumber)
            .IsUnique();

        builder.Property(t => t.TransactionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength()
            .IsUnicode(false);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();

        builder.Property(t => t.CompletedAtUtc)
            .IsRequired(false);

        builder.HasIndex(t => new { t.SourceAccountId, t.CreatedAtUtc });
        builder.HasIndex(t => new { t.DestinationAccountId, t.CreatedAtUtc });
        builder.HasIndex(t => t.CreatedAtUtc);
        builder.HasIndex(t => t.Status);

        builder.HasOne(t => t.SourceAccount)
            .WithMany(ba => ba.SourceTransactions)
            .HasForeignKey(t => t.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.DestinationAccount)
            .WithMany(ba => ba.DestinationTransactions)
            .HasForeignKey(t => t.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
