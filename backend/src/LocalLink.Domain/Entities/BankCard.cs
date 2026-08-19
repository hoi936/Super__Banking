using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class BankCard
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public Guid LinkedAccountId { get; set; }
    public virtual BankAccount LinkedAccount { get; set; } = null!;

    public string CardNumberMasked { get; set; } = string.Empty;
    public string LastFourDigits { get; set; } = string.Empty;
    public string CardholderName { get; set; } = string.Empty;
    public CardType CardType { get; set; } = CardType.Debit;
    public CardStatus Status { get; set; } = CardStatus.Active;

    public decimal DailyLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public string Currency { get; set; } = "VND";
    public bool OnlinePaymentEnabled { get; set; } = true;
    public bool ContactlessEnabled { get; set; } = true;

    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
