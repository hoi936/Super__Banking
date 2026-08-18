namespace LocalLink.Application.Beneficiaries.DTOs;

public class CreateBeneficiaryRequest
{
    public string AccountNumber { get; init; } = string.Empty;
    public string? Nickname { get; init; }
}
