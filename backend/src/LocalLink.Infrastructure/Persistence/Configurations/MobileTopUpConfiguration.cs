using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class MobileTopUpConfiguration : IEntityTypeConfiguration<MobileTopUp>
{
    public void Configure(EntityTypeBuilder<MobileTopUp> builder)
    {
        builder.ToTable("MobileTopUps", t =>
        {
            t.HasCheckConstraint("CK_MobileTopUps_Amount_Positive", "[Amount] > 0");
        });

        builder.HasKey(topUp => topUp.Id);

        builder.Property(topUp => topUp.TransactionId)
            .IsRequired();

        builder.HasIndex(topUp => topUp.TransactionId)
            .IsUnique();

        builder.Property(topUp => topUp.ProviderCode)
            .IsRequired()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(topUp => topUp.ProviderName)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(topUp => topUp.ProductType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(topUp => topUp.ProductCode)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(topUp => topUp.ProductName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(topUp => topUp.PhoneNumber)
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(topUp => topUp.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(topUp => topUp.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength()
            .IsUnicode(false);

        builder.Property(topUp => topUp.CardSerial)
            .HasMaxLength(40)
            .IsUnicode(false);

        builder.Property(topUp => topUp.CardPin)
            .HasMaxLength(40)
            .IsUnicode(false);

        builder.Property(topUp => topUp.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(topUp => topUp.IdempotencyKey)
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.HasIndex(topUp => topUp.IdempotencyKey)
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL");

        builder.Property(topUp => topUp.CreatedAtUtc)
            .IsRequired();

        builder.Property(topUp => topUp.CompletedAtUtc)
            .IsRequired(false);

        builder.HasIndex(topUp => new { topUp.CustomerId, topUp.CreatedAtUtc });
        builder.HasIndex(topUp => topUp.SourceAccountId);
        builder.HasIndex(topUp => topUp.Status);
        builder.HasIndex(topUp => topUp.ProviderCode);

        builder.HasOne(topUp => topUp.Customer)
            .WithMany(customer => customer.MobileTopUps)
            .HasForeignKey(topUp => topUp.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(topUp => topUp.SourceAccount)
            .WithMany(account => account.MobileTopUps)
            .HasForeignKey(topUp => topUp.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(topUp => topUp.Transaction)
            .WithOne(transaction => transaction.MobileTopUp)
            .HasForeignKey<MobileTopUp>(topUp => topUp.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
