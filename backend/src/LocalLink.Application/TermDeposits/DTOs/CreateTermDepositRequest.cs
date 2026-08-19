using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.TermDeposits.DTOs;

public class CreateTermDepositRequest
{
    [Required]
    public Guid SourceAccountId { get; set; }

    [Range(100000, 100000000000)]
    public decimal PrincipalAmount { get; set; }

    [Required]
    public int TenorMonths { get; set; }
}
