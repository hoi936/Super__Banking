using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ReferenceNumber { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    
    public Guid? SourceAccountId { get; set; }
    public virtual BankAccount? SourceAccount { get; set; }

    public Guid? DestinationAccountId { get; set; }
    public virtual BankAccount? DestinationAccount { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? Description { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }

    // Navigation properties
    public virtual Transfer? Transfer { get; set; }
    public virtual Payment? Payment { get; set; }
    public virtual ExternalTransfer? ExternalTransfer { get; set; }
    public virtual LoanApplication? LoanApplication { get; set; }
    public virtual MobileTopUp? MobileTopUp { get; set; }
}
