namespace LocalLink.Application.TermDeposits.DTOs;

public class TermDepositReceiptDto
{
    public Guid TermDepositId { get; set; }
    public string DepositNumber { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
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
}
