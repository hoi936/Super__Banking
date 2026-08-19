namespace LocalLink.Application.MobileTopUps.DTOs;

public class MobileTopUpReceiptDto
{
    public Guid MobileTopUpId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string ProviderCode { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal RemainingBalance { get; set; }
    public string Currency { get; set; } = "VND";
    public string? CardSerial { get; set; }
    public string? CardPin { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
