using LocalLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalLink.Infrastructure.Persistence.Configurations;

public class BankCardConfiguration : IEntityTypeConfiguration<BankCard>
{
    public void Configure(EntityTypeBuilder<BankCard> builder)
    {
        builder.ToTable("Cards", t =>
        {
            t.HasCheckConstraint("CK_Cards_DailyLimit_Positive", "[DailyLimit] > 0");
            t.HasCheckConstraint("CK_Cards_MonthlyLimit_Positive", "[MonthlyLimit] > 0");
            t.HasCheckConstraint("CK_Cards_MonthlyLimit_Gte_DailyLimit", "[MonthlyLimit] >= [DailyLimit]");
            t.HasCheckConstraint("CK_Cards_ExpiryMonth_Valid", "[ExpiryMonth] BETWEEN 1 AND 12");
            t.HasCheckConstraint("CK_Cards_ExpiryYear_Valid", "[ExpiryYear] BETWEEN 2026 AND 2099");
        });

        builder.HasKey(card => card.Id);

        builder.Property(card => card.CardNumberMasked)
            .IsRequired()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.HasIndex(card => card.CardNumberMasked)
            .IsUnique();

        builder.Property(card => card.LastFourDigits)
            .IsRequired()
            .HasMaxLength(4)
            .IsUnicode(false);

        builder.Property(card => card.CardholderName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(card => card.CardType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(card => card.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(card => card.DailyLimit)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(card => card.MonthlyLimit)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(card => card.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .IsFixedLength()
            .IsUnicode(false);

        builder.Property(card => card.OnlinePaymentEnabled)
            .IsRequired();

        builder.Property(card => card.ContactlessEnabled)
            .IsRequired();

        builder.Property(card => card.ExpiryMonth)
            .IsRequired();

        builder.Property(card => card.ExpiryYear)
            .IsRequired();

        builder.Property(card => card.IssuedAtUtc)
            .IsRequired();

        builder.Property(card => card.CreatedAtUtc)
            .IsRequired();

        builder.Property(card => card.UpdatedAtUtc)
            .IsRequired(false);

        builder.HasIndex(card => new { card.CustomerId, card.Status });
        builder.HasIndex(card => card.LinkedAccountId);

        builder.HasOne(card => card.Customer)
            .WithMany(customer => customer.Cards)
            .HasForeignKey(card => card.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(card => card.LinkedAccount)
            .WithMany(account => account.Cards)
            .HasForeignKey(card => card.LinkedAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
