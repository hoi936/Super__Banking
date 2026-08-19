using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.Napas.DTOs;

public class NapasLookupRequest
{
    [Required]
    [MaxLength(20)]
    public string BankCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string DestinationNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression("ACCOUNT|CARD", ErrorMessage = "Destination type must be ACCOUNT or CARD.")]
    public string DestinationType { get; set; } = "ACCOUNT";
}
