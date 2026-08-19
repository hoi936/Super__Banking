using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class LoanApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public Guid DisbursementAccountId { get; set; }
    public virtual BankAccount DisbursementAccount { get; set; } = null!;

    public Guid? ReviewedByUserId { get; set; }
    public virtual User? ReviewedByUser { get; set; }

    public Guid? DisbursementTransactionId { get; set; }
    public virtual Transaction? DisbursementTransaction { get; set; }

    public string ApplicationNumber { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal MonthlyIncome { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Currency { get; set; } = "VND";
    public LoanApplicationStatus Status { get; set; } = LoanApplicationStatus.Pending;
    public string? ReviewNote { get; set; }

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAtUtc { get; set; }
    public DateTime? DisbursedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
