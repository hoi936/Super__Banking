using LocalLink.Domain.Enums;

namespace LocalLink.Application.Admin.DTOs;

public class AdminPaymentListItemDto
{
    public Guid Id { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public BillType BillType { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
