using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.Loans.DTOs;

public class ApproveLoanApplicationRequest
{
    [Required]
    [Range(1000000, 1000000000, ErrorMessage = "Approved amount must be between 1,000,000 and 1,000,000,000 VND.")]
    public decimal ApprovedAmount { get; set; }

    [Required]
    [Range(0.01, 100, ErrorMessage = "Annual interest rate must be positive.")]
    public decimal AnnualInterestRate { get; set; }

    [MaxLength(500)]
    public string? ReviewNote { get; set; }
}
