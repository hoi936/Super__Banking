namespace LocalLink.Application.TermDeposits.DTOs;

public class TermDepositDto
{
    public Guid Id { get; set; }
    public string DepositNumber { get; set; } = string.Empty;
    public Guid SourceAccountId { get; set; }
    public string SourceAccountNumber { get; set; } = string.Empty;
    public decimal PrincipalAmount { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public int TenorMonths { get; set; }
    public decimal ExpectedInterestAmount { get; set; }
    public decimal ExpectedPayoutAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = string.Empty;
    public DateTime OpenedAtUtc { get; set; }
    public DateTime MaturityDateUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
}
