namespace LocalLink.Application.Payments.DTOs;

public class PaymentListItemDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string BillNumber { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string BillType { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
