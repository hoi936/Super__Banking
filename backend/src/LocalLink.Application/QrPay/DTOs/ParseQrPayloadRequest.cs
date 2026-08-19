using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.QrPay.DTOs;

public class ParseQrPayloadRequest
{
    [Required]
    [MaxLength(2000)]
    public string Payload { get; set; } = string.Empty;
}
