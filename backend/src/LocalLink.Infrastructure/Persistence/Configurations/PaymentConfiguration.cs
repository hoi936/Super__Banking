using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", t =>
        {
            t.HasCheckConstraint("CK_Payments_Amount_Positive", "[Amount] > 0");
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.BillId)
            .IsRequired();

        builder.HasIndex(p => p.BillId)
            .IsUnique();

        builder.Property(p => p.AccountId)
            .IsRequired();

        builder.Property(p => p.TransactionId)
            .IsRequired();

        builder.HasIndex(p => p.TransactionId)
            .IsUnique();

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(p => p.PaidAtUtc)
            .IsRequired(false);

        builder.Property(p => p.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(p => p.Bill)
            .WithOne(b => b.Payment)
            .HasForeignKey<Payment>(p => p.BillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Account)
            .WithMany(ba => ba.Payments)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Transaction)
            .WithOne(t => t.Payment)
            .HasForeignKey<Payment>(p => p.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
