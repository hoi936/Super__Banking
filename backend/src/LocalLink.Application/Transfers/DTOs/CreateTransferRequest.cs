using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.Transfers.DTOs;

public class CreateTransferRequest
{
    [Required]
    public Guid SourceAccountId { get; set; }

    [Required]
    [MaxLength(50)]
    public string DestinationAccountNumber { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
