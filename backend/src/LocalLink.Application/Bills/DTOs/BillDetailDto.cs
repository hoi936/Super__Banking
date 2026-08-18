namespace LocalLink.Application.Bills.DTOs;

public class BillDetailDto
{
    public Guid Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string BillType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? PaymentId { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}
