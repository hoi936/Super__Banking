namespace LocalLink.Application.PaymentGateway.DTOs;

public class GatewayDepositReceiptDto
{
    public Guid TransactionId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
