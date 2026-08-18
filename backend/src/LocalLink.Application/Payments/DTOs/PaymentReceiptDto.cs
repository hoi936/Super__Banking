namespace LocalLink.Application.Payments.DTOs;

public class PaymentReceiptDto
{
    public Guid PaymentId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string BillNumber { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAtUtc { get; set; }
}
