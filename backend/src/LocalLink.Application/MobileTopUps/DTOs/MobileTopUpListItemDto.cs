namespace LocalLink.Application.MobileTopUps.DTOs;

public class MobileTopUpListItemDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
