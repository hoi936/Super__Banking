using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.Loans.DTOs;

public class CreateLoanApplicationRequest
{
    [Required]
    public Guid DisbursementAccountId { get; set; }

    [Required]
    [Range(1000000, 1000000000, ErrorMessage = "Requested amount must be between 1,000,000 and 1,000,000,000 VND.")]
    public decimal RequestedAmount { get; set; }

    [Required]
    [Range(6, 60, ErrorMessage = "Term must be between 6 and 60 months.")]
    public int TermMonths { get; set; }

    [Required]
    [Range(1000000, 1000000000, ErrorMessage = "Monthly income must be positive.")]
    public decimal MonthlyIncome { get; set; }

    [Required]
    [MaxLength(500)]
    public string Purpose { get; set; } = string.Empty;
}
