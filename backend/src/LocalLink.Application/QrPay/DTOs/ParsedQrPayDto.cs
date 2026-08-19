namespace LocalLink.Application.QrPay.DTOs;

public class ParsedQrPayDto
{
    public bool IsSupported { get; init; }
    public string PaymentRail { get; init; } = string.Empty;
    public string BankCode { get; init; } = string.Empty;
    public string BankName { get; init; } = string.Empty;
    public string AccountNumber { get; init; } = string.Empty;
    public string? AccountName { get; init; }
    public decimal? Amount { get; init; }
    public string? Description { get; init; }
    public string RawPayload { get; init; } = string.Empty;
    public string? Warning { get; init; }
}
