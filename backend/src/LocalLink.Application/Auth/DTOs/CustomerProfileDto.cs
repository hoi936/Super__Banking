namespace LocalLink.Application.Auth.DTOs;

public class CustomerProfileDto
{
    public Guid Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
