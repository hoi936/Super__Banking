namespace LocalLink.Application.Transactions.DTOs;

public class TransactionListItemDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public Guid? SourceAccountId { get; set; }
    public string? SourceAccountNumber { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
