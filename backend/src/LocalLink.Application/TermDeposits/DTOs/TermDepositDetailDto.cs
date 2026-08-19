namespace LocalLink.Application.TermDeposits.DTOs;

public class TermDepositDetailDto : TermDepositDto
{
    public Guid OpeningTransactionId { get; set; }
    public Guid? MaturityTransactionId { get; set; }
    public decimal? PaidInterestAmount { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
