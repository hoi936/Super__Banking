using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class Transfer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TransactionId { get; set; }
    public virtual Transaction Transaction { get; set; } = null!;

    public Guid SourceAccountId { get; set; }
    public virtual BankAccount SourceAccount { get; set; } = null!;

    public Guid DestinationAccountId { get; set; }
    public virtual BankAccount DestinationAccount { get; set; } = null!;

    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? IdempotencyKey { get; set; }
    public TransferStatus Status { get; set; } = TransferStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}
