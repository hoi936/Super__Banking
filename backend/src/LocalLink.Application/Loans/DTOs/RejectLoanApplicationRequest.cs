using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.Loans.DTOs;

public class RejectLoanApplicationRequest
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
