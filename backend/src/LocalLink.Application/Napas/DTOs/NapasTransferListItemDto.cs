namespace LocalLink.Application.Napas.DTOs;

public class NapasTransferListItemDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string ExternalBankCode { get; set; } = string.Empty;
    public string ExternalBankName { get; set; } = string.Empty;
    public string DestinationNumber { get; set; } = string.Empty;
    public string DestinationName { get; set; } = string.Empty;
    public string DestinationType { get; set; } = "ACCOUNT";
    public decimal Amount { get; set; }
    public decimal FeeAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
