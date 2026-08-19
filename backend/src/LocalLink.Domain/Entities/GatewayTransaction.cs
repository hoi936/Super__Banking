using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class GatewayTransaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public GatewayProvider Provider { get; set; }
    
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    
    public GatewayTransactionStatus Status { get; set; }
    
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? GatewayTransactionId { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    
    // Navigation Property
    public virtual BankAccount? Account { get; set; }
}
