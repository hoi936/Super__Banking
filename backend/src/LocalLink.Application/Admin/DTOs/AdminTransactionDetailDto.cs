using LocalLink.Domain.Enums;

namespace LocalLink.Application.Admin.DTOs;

public class AdminTransactionDetailDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public TransactionStatus Status { get; set; }
    public string? Description { get; set; }

    public AdminTransactionAccountDto? SourceAccount { get; set; }
    public AdminTransactionAccountDto? DestinationAccount { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public class AdminTransactionAccountDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string CustomerFullName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
}
