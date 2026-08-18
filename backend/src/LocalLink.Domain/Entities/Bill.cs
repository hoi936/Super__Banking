using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class Bill
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public string ProviderName { get; set; } = string.Empty;
    public BillType BillType { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public BillStatus Status { get; set; } = BillStatus.Unpaid;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation property
    public virtual Payment? Payment { get; set; }
}
