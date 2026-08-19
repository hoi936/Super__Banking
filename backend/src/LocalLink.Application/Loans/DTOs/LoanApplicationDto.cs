namespace LocalLink.Application.Loans.DTOs;

public class LoanApplicationDto
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerFullName { get; set; } = string.Empty;
    public Guid DisbursementAccountId { get; set; }
    public string DisbursementAccountNumber { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal MonthlyIncome { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = string.Empty;
    public string? ReviewNote { get; set; }
    public string? DisbursementReference { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public DateTime? DisbursedAtUtc { get; set; }
}
