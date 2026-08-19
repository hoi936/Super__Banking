using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class ExternalTransfer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TransactionId { get; set; }
    public virtual Transaction Transaction { get; set; } = null!;

    public Guid SourceAccountId { get; set; }
    public virtual BankAccount SourceAccount { get; set; } = null!;

    public string ExternalBankCode { get; set; } = string.Empty;
    public string ExternalBankName { get; set; } = string.Empty;
    public string DestinationAccountNumber { get; set; } = string.Empty;
    public string DestinationAccountName { get; set; } = string.Empty;
    public string DestinationType { get; set; } = "ACCOUNT";

    public decimal Amount { get; set; }
    public decimal FeeAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? Description { get; set; }
    public string? IdempotencyKey { get; set; }
    public ExternalTransferStatus Status { get; set; } = ExternalTransferStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}
