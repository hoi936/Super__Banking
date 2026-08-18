namespace LocalLink.Application.Accounts.DTOs;

public class AccountSummaryDto
{
    public Guid Id { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string AccountType { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public string Currency { get; init; } = "VND";
    public string Status { get; init; } = string.Empty;
}
