namespace LocalLink.Application.Napas.DTOs;

public class NapasLookupResultDto
{
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string DestinationNumber { get; set; } = string.Empty;
    public string DestinationName { get; set; } = string.Empty;
    public string DestinationType { get; set; } = "ACCOUNT";
}
