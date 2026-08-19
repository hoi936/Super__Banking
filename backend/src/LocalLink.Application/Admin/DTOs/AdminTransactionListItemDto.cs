using LocalLink.Domain.Enums;

namespace LocalLink.Application.Admin.DTOs;

public class AdminTransactionListItemDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public TransactionStatus Status { get; set; }
    public string? SourceAccountNumber { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
