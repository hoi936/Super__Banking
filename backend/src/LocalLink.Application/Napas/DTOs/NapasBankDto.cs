namespace LocalLink.Application.Napas.DTOs;

public class NapasBankDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public bool SupportsAccountTransfer { get; set; } = true;
    public bool SupportsCardTransfer { get; set; } = true;
}
