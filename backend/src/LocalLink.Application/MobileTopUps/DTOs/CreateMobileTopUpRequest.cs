using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.MobileTopUps.DTOs;

public class CreateMobileTopUpRequest
{
    [Required]
    public Guid SourceAccountId { get; set; }

    [Required]
    [MaxLength(30)]
    public string ProviderCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}
