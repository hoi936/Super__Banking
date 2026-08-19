namespace LocalLink.Application.QrPay.DTOs;

public class QrPayPayloadDto
{
    public string Payload { get; init; } = string.Empty;
    public string PayloadFormat { get; init; } = "LOCALBANK";
    public Guid AccountId { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string BankCode { get; init; } = string.Empty;
    public string BankName { get; init; } = string.Empty;
    public decimal? Amount { get; init; }
    public string? Description { get; init; }
    public DateTime GeneratedAtUtc { get; init; }
}
