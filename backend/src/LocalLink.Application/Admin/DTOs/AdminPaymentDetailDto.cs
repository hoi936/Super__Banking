using LocalLink.Domain.Enums;

namespace LocalLink.Application.Admin.DTOs;

public class AdminPaymentDetailDto
{
    public Guid Id { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    
    public string CustomerFullName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    
    public string BillNumber { get; set; } = string.Empty;
    public BillType BillType { get; set; }
    public string ProviderName { get; set; } = string.Empty;

    public string? TransactionReference { get; set; }
    public string? IdempotencyKey { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}
