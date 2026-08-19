using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class TermDeposit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public Guid SourceAccountId { get; set; }
    public virtual BankAccount SourceAccount { get; set; } = null!;

    public Guid OpeningTransactionId { get; set; }
    public virtual Transaction OpeningTransaction { get; set; } = null!;

    public Guid? MaturityTransactionId { get; set; }
    public virtual Transaction? MaturityTransaction { get; set; }

    public string DepositNumber { get; set; } = string.Empty;
    public decimal PrincipalAmount { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public int TenorMonths { get; set; }
    public decimal ExpectedInterestAmount { get; set; }
    public decimal? PaidInterestAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public TermDepositStatus Status { get; set; } = TermDepositStatus.Active;
    public string? IdempotencyKey { get; set; }

    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime MaturityDateUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
