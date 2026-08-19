using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.Napas.DTOs;

public class CreateNapasTransferRequest
{
    [Required]
    public Guid SourceAccountId { get; set; }

    [Required]
    [MaxLength(20)]
    public string BankCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string DestinationNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression("ACCOUNT|CARD", ErrorMessage = "Destination type must be ACCOUNT or CARD.")]
    public string DestinationType { get; set; } = "ACCOUNT";

    [Required]
    [Range(1000, 500000000, ErrorMessage = "Amount must be between 1,000 and 500,000,000 VND.")]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
