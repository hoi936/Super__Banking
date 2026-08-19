namespace LocalLink.Application.Transfers.DTOs;

public class TransferListItemDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public Guid SourceAccountId { get; set; }
    public string SourceAccountNumber { get; set; } = string.Empty;
    public Guid DestinationAccountId { get; set; }
    public string DestinationAccountNumber { get; set; } = string.Empty;
    public string DestinationAccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
