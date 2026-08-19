namespace LocalLink.Application.Beneficiaries.DTOs;

public class BeneficiaryDto
{
    public Guid Id { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string? Nickname { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
