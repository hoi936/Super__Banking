using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("Transfers", t =>
        {
            t.HasCheckConstraint("CK_Transfers_Amount_Positive", "[Amount] > 0");
        });

        builder.HasKey(tr => tr.Id);

        builder.Property(tr => tr.TransactionId)
            .IsRequired();

        builder.HasIndex(tr => tr.TransactionId)
            .IsUnique();

        builder.Property(tr => tr.SourceAccountId)
            .IsRequired();

        builder.Property(tr => tr.DestinationAccountId)
            .IsRequired();

        builder.Property(tr => tr.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(tr => tr.Description)
            .HasMaxLength(500);

        builder.Property(tr => tr.IdempotencyKey)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.HasIndex(tr => tr.IdempotencyKey)
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL");

        builder.Property(tr => tr.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(tr => tr.CreatedAtUtc)
            .IsRequired();

        builder.Property(tr => tr.CompletedAtUtc)
            .IsRequired(false);

        builder.HasOne(tr => tr.Transaction)
            .WithOne(t => t.Transfer)
            .HasForeignKey<Transfer>(tr => tr.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tr => tr.SourceAccount)
            .WithMany(ba => ba.SourceTransfers)
            .HasForeignKey(tr => tr.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tr => tr.DestinationAccount)
            .WithMany(ba => ba.DestinationTransfers)
            .HasForeignKey(tr => tr.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
