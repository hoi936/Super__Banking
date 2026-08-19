namespace LocalLink.Application.Cards.DTOs;

public class CardDto
{
    public Guid Id { get; set; }
    public Guid LinkedAccountId { get; set; }
    public string LinkedAccountNumber { get; set; } = string.Empty;
    public string CardNumberMasked { get; set; } = string.Empty;
    public string LastFourDigits { get; set; } = string.Empty;
    public string CardholderName { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal DailyLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public string Currency { get; set; } = "VND";
    public bool OnlinePaymentEnabled { get; set; }
    public bool ContactlessEnabled { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public DateTime IssuedAtUtc { get; set; }
}
