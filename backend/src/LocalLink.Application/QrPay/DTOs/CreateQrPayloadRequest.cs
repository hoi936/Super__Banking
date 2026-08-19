using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.QrPay.DTOs;

public class CreateQrPayloadRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Range(0, 999_999_999_999)]
    public decimal? Amount { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }
}
