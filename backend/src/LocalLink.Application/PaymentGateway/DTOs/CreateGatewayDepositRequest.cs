using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.PaymentGateway.DTOs;

public class CreateGatewayDepositRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    [Range(50000, 1000000000)]
    public decimal Amount { get; set; }
    
    [Required]
    public string Provider { get; set; } = string.Empty;
}
