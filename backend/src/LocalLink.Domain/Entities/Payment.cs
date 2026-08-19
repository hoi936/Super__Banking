using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BillId { get; set; }
    public virtual Bill Bill { get; set; } = null!;

    public Guid AccountId { get; set; }
    public virtual BankAccount Account { get; set; } = null!;

    public Guid TransactionId { get; set; }
    public virtual Transaction Transaction { get; set; } = null!;

    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? IdempotencyKey { get; set; }

    public DateTime? PaidAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
