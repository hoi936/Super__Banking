using LocalLink.Domain.Enums;

namespace LocalLink.Domain.Entities;

public class MobileTopUp
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public Guid SourceAccountId { get; set; }
    public virtual BankAccount SourceAccount { get; set; } = null!;

    public Guid TransactionId { get; set; }
    public virtual Transaction Transaction { get; set; } = null!;

    public string ProviderCode { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public MobileTopUpProductType ProductType { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? CardSerial { get; set; }
    public string? CardPin { get; set; }
    public MobileTopUpStatus Status { get; set; } = MobileTopUpStatus.Pending;
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}
