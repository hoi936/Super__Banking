using System.ComponentModel.DataAnnotations;

namespace LocalLink.Application.PaymentGateway.DTOs;

public class GatewayCallbackRequest
{
    [Required]
    public string ReferenceNumber { get; set; } = string.Empty;
    
    [Required]
    public string Provider { get; set; } = string.Empty;
    
    [Required]
    public string Status { get; set; } = string.Empty; // "Success" or "Failed"
    
    public string? GatewayTransactionId { get; set; }
}
